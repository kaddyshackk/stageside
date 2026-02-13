using StageSide.Ticketmaster.Service;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<TicketmasterWorker>();
builder.Services.AddHttpClient<TicketmasterWorker>(client =>
{
	client.BaseAddress = new Uri("https://api.example.com");
	client.Timeout = TimeSpan.FromSeconds(30);
});

var host = builder.Build();
host.Run();
