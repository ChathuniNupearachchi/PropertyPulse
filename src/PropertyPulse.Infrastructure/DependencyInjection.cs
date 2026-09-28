using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PropertyPulse.Infrastructure.Data;
using PropertyPulse.Infrastructure.Repositories;
using PropertyPulse.Infrastructure.Storage;

namespace PropertyPulse.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();

        services.AddSingleton<IOptions<ImageStorageOptions>>(_ => Options.Create(new ImageStorageOptions
        {
            RootPath = configuration[$"{ImageStorageOptions.SectionName}:RootPath"] ?? string.Empty
        }));
        services.AddSingleton<IPropertyImageStorage, PropertyImageStorage>();

        return services;
    }
}
