using Microsoft.EntityFrameworkCore;
using PropertyPulse.Api.Seeding;
using PropertyPulse.Infrastructure.Data;

namespace PropertyPulse.Api.Extensions;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeDevelopmentDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        await scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>().SeedAsync();
    }
}
