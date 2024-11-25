using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;

namespace GrpcClientHttpGateway.CustomMiddleware;

public class GrpcExceptionHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            await WriteProblemDetailsResponse(
                context,
                StatusCodes.Status404NotFound,
                "Resource not found.");
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument)
        {
            await WriteProblemDetailsResponse(
                context,
                StatusCodes.Status400BadRequest,
                "Invalid request arguments.");
        }
        catch (RpcException)
        {
            await WriteProblemDetailsResponse(
                context,
                StatusCodes.Status500InternalServerError,
                "An internal server error occurred.");
        }
        catch (Exception)
        {
            await WriteProblemDetailsResponse(
                context,
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private async Task WriteProblemDetailsResponse(HttpContext context, int statusCode, string title)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = ReasonPhrases.GetReasonPhrase(statusCode),
            Instance = context.Request.Path,
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}