using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Options;

namespace HekyLab.PingTray.App;

public class PingWorker(
  ILogger<PingWorker> logger,
  IOptions<AppSettings> appOptions,
  IPingResultsStorage resultsStorage
) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      await DoWork(stoppingToken);
      await Task.Delay(appOptions.Value.PingDelay, stoppingToken);
    }
  }

  private async Task DoWork(CancellationToken cancellationToken)
  {
    var hosts = appOptions.Value.Hosts;
    logger.LogInformation("Pinging {count} hosts...", hosts.Count());

    if (!hosts.Any()) logger.LogWarning("No hosts to ping!");

    var tasks = hosts.Select(async address =>
    {
      if (cancellationToken.IsCancellationRequested) return;
      using var ping = new Ping();
      logger.LogInformation("Pinging {address}...", address);
      var reply = await ping.SendPingAsync(address, TimeSpan.FromSeconds(1), cancellationToken: cancellationToken);
      logger.LogInformation("Ping {address} status: {reply.Status}", address, reply.Status);
      resultsStorage.Store(address, reply.Status);
    });

    await Task.WhenAll(tasks);
  }
}
