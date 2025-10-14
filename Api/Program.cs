using ComedyPull.Api.Extensions;
using ComedyPull.Api.Modules.Public;
using ComedyPull.Application.Extensions;
using ComedyPull.Data.Extensions;
using Microsoft.Playwright;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// -- [ Configure Services ] ----

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("Settings/appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"Settings/appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddOpenApi();
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddDataServices(builder.Configuration);

// -- [ Configure Application ] ----

var app = builder.Build();

await VerifyPlaywrightAsync();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapGroup("/api")
    .AddPublicEndpoints();

app.Run();

return 0;

// -- [ Static Helper Methods ] ----

static async Task VerifyPlaywrightAsync()
{
    try
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        await browser.CloseAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Playwright verification failed: {ex.Message}");
        Environment.Exit(789);
    }
}