using Task1.Mappers;

namespace Task1.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddPl(this IServiceCollection services)
    {
        services.AddScoped<PlMapper>();

        return services;
    }
}