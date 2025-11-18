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
builder.Services.AddDomainLayer();
builder.Services.AddSources();

// -- [ Configure Application ] ----

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.MapEndpoints(typeof(Program).Assembly);

app.Run();

return 0;
