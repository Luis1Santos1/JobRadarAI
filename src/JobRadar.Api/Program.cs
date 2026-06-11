using JobRadar.Application;
using JobRadar.Infrastructure;
using JobRadar.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "JobRadar.Api")
        .WriteTo.Console(new RenderedCompactJsonFormatter());
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<JobRadarDbContext>("sqlserver");

var app = builder.Build();

await ApplyMigrationsAsync(app);

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString());
    };
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    Application = "JobRadar AI API",
    Status = "Running",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTimeOffset.UtcNow
}));

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready");

app.MapControllers();

app.Run();

static async Task ApplyMigrationsAsync(WebApplication app)
{
    var applyMigrations = app.Configuration.GetValue<bool>("Database:ApplyMigrations");

    if (!applyMigrations)
    {
        return;
    }

    using var scope = app.Services.CreateScope();

    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<JobRadarDbContext>();

    const int maxAttempts = 10;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            logger.LogInformation("Applying database migrations. Attempt {Attempt}/{MaxAttempts}", attempt, maxAttempts);

            await dbContext.Database.MigrateAsync();

            logger.LogInformation("Database migrations applied successfully.");

            return;
        }
        catch (Exception exception) when (attempt < maxAttempts)
        {
            logger.LogWarning(
                exception,
                "Failed to apply database migrations. Retrying in 5 seconds. Attempt {Attempt}/{MaxAttempts}",
                attempt,
                maxAttempts);

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    await dbContext.Database.MigrateAsync();
}