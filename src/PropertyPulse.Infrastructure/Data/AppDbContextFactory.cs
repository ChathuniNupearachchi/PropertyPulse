using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PropertyPulse.Infrastructure.Data;

// Used only by dotnet-ef, so migrations can be created without starting the API host.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=propertypulse;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(options);
    }
}
