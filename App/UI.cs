using System.Diagnostics;

namespace HekyLab.PingTray.App;

public class UI(ILogger<UI> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    using var process = new Process
    {
      StartInfo = new ProcessStartInfo
      {
        FileName = "client.exe",
        UseShellExecute = false,
        CreateNoWindow = false,
      }
    };

    process.Start();
    await process.WaitForExitAsync(stoppingToken);
  }

}
