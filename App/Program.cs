
using HekyLab.PingTray.App;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
  .Configure<AppSettings>(builder.Configuration.GetSection("App"))
  .AddHostedService<Server>()
  // .AddHostedService<UI>()
  .AddHostedService<PingWorker>()
  .AddSingleton<IPingResultsStorage, MemoryPingResultsStorage>();

var host = builder.Build();
await host.RunAsync();
