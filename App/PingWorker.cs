using Microsoft.Extensions.Options;
using System.Net.NetworkInformation;

namespace HekyLab.PingTray.App;

public class PingWorker(
  ILogger<PingWorker> logger,
  IOptions<AppSettings> appOptions,
  IPingResultsStorage resultsStorage
) : BackgroundService
{
  private IEnumerable<string> Hosts => appOptions.Value.Hosts;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    if (Hosts.Any()) logger.LogInformation("Configured {count} hosts", Hosts.Count());
    else logger.LogWarning("No hosts to ping!");

    while (!stoppingToken.IsCancellationRequested)
    {
      await DoWork(stoppingToken);
      await Task.Delay(appOptions.Value.PingDelay, stoppingToken);
    }
  }

  private async Task DoWork(CancellationToken cancellationToken)
  {
    var tasks = Hosts.Select(async address =>
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
