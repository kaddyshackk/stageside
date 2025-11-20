using Coravel;
using Coravel.Queuing.Interfaces;
using Microsoft.Playwright;
using Scalar.AspNetCore;
using Serilog;
using StageSide.SpaCollector.Data.Extensions;
using StageSide.SpaCollector.Domain.Extensions;
using StageSide.SpaCollector.Service.Extensions;

var builder = WebApplication.CreateBuilder(args);

// -- [ Configure Services ] ----

builder.Services.AddOpenApi();

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
builder.Services.AddDomainLayer(builder.Configuration);
builder.Services.AddSources();

// -- [ Configure Application ] ----

var app = builder.Build();

await VerifyPlaywrightAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.MapEndpoints(typeof(Program).Assembly);
app.Services.ConfigureQueue()
    .LogQueuedTaskProgress(app.Services.GetRequiredService<ILogger<IQueue>>());

app.Run();

return 0;

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