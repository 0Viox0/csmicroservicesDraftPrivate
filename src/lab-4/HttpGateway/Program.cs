using GrpcClientHttpGateway.CustomMiddleware;
using GrpcClientHttpGateway.Extensions;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "GrpcServiceUrls.json",
    optional: false,
    reloadOnChange: true);

builder.Services
    .AddOrdersServiceOptions(builder.Configuration)
    .AddHttpOrdersGrpcClient()
    .AddHttpProductsGrpcClient()
    .AddHttpOrderProcessingGrpcClient()
    .AddGrpcModelMapper();

string xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Order management APi",
        Description = "An asp.net API for managing orders",
    });
    c.IncludeXmlComments(xmlPath);
});

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.UseMiddleware<GrpcExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();