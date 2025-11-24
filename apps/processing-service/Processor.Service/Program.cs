using Serilog;
using StageSide.Processor.Data.Extensions;
using StageSide.Processor.Domain.Extensions;
using StageSide.Processor.Service.Extensions;

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
builder.Services.AddSources(builder.Configuration);

// -- [ Configure Application ] ----

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

app.Run();

return 0;