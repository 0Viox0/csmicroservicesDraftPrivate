using Grpc.Core;
using Grpc.Core.Interceptors;
using Task3.Bll.CustomExceptions;

namespace GrpcServer.Interceptors;

public class ErrorHandlingInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (OrderException ex)
        {
            var errorDetail = new RpcException(
                new Status(StatusCode.InvalidArgument, ex.Message),
                new Metadata
                {
                    { "error-message", ex.Message },
                    { "error-type", "OrderException" },
                });

            throw errorDetail;
        }
        catch (Exception ex)
        {
            var errorDetail = new RpcException(
                new Status(StatusCode.Internal, "An internal error occurred."),
                new Metadata
                {
                    { "error-message", ex.Message },
                    { "error-type", "GeneralException" },
                });

            throw errorDetail;
        }
    }
}