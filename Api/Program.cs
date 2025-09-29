using ComedyPull.Api.Extensions;
using ComedyPull.Application.Extensions;
using ComedyPull.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;

var builder = WebApplication.CreateBuilder(args);

// -- [ Configure Services ] ----

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("Settings/appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"Settings/appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddDataServices(builder.Configuration);

// -- [ Configure Application ] ----

var app = builder.Build();

await VerifyPlaywrightAsync();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

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