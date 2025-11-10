using StageSide.Pipeline.Data.Extensions;
using StageSide.Pipeline.Domain.Extensions;
using StageSide.Pipeline.Service.Extensions;
using Microsoft.Playwright;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// -- [ Configure Services ] ----

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("Settings/appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"Settings/appsettings.{builder.Environment.EnvironmentName}.json", optional: true,
        reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddOpenApi();
builder.Services.AddServiceLayer(builder.Configuration);
builder.Services.AddDataLayer(builder.Configuration);
builder.Services.AddDomainLayer();

// -- [ Configure Application ] ----

var app = builder.Build();

await VerifyPlaywrightAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapEndpoints(typeof(Program).Assembly);

app.Run();

return 0;

// -- [ Static Helper Methods ] ----

static async Task VerifyPlaywrightAsync()
{
    try
    {
        Log.Information("Verifying playwright installation.");
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        await browser.CloseAsync();
        Log.Debug("Verified playwright installation.");
    }
    catch (Exception ex)
    {
        Log.Fatal("Playwright verification failed: {Error}", ex.Message);
        throw new InvalidOperationException("Playwright verification failed. Browser binaries are not installed.");
    }
}