using System.Diagnostics;

namespace HekyLab.PingTray.App;

public class Client(ILogger<Client> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    using var process = new Process
    {
      StartInfo = new ProcessStartInfo
      {
        FileName = "client.exe", // Název druhé aplikace
        UseShellExecute = false,    // Nutné pro sdílenou konzoli
        CreateNoWindow = false,     // Pokud nechceš novou konzoli
      }
    };

    process.Start();
    await process.WaitForExitAsync(stoppingToken);
  }

}
