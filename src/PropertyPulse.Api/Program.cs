using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using PropertyPulse.Api.ExceptionHandling;
using PropertyPulse.Api.Extensions;
using PropertyPulse.Api.Filters;
using PropertyPulse.Api.Seeding;
using PropertyPulse.Api.Services;
using PropertyPulse.Api.Validators;
using PropertyPulse.Domain.Entities;
using PropertyPulse.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured.");
    var imageStorageRootPath = builder.Configuration["ImageStorage:RootPath"]
        ?? throw new InvalidOperationException("ImageStorage:RootPath is not configured.");

    builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());
    builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddSwagger();

    builder.Services.AddInfrastructure(connectionString, builder.Configuration);
    builder.Services.AddJwtAuthentication();

    builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserService, UserService>();

    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddScoped<DevelopmentDataSeeder>();
    }

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        await app.InitializeDevelopmentDatabaseAsync();
    }

    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();
    app.UseStatusCodePages();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    Directory.CreateDirectory(imageStorageRootPath);

    // Served without authentication: the URLs are opaque (GUID file names) and are only ever handed out
    // through the property endpoints, which already require a signed-in caller.
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(imageStorageRootPath),
        RequestPath = "/images/properties"
    });

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "API terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
