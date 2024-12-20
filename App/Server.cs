using System.Globalization;
using System.IO.Pipes;

namespace HekyLab.PingTray.App;

public class Server(ILogger<Server> logger, IPingResultsStorage resultsStorage) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      using var stream = new NamedPipeServerStream("HekyLab.PingTray", PipeDirection.InOut);

      logger.LogInformation("Waiting for connection...");
      await stream.WaitForConnectionAsync(stoppingToken);
      logger.LogInformation("Connected");

      try
      {
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream) { AutoFlush = true };

        while (!reader.EndOfStream && !stoppingToken.IsCancellationRequested)
        {
          var request = await reader.ReadLineAsync(stoppingToken);
          logger.LogInformation("Received request: {request}", request);

          var response = ProcessRequest(request);

          logger.LogInformation("Sending response: {response}", response);
          await writer.WriteLineAsync(response);
        }
      }
      catch (IOException ex)
      {
        logger.LogError(ex, "Error while processing request");
      }
      finally
      {
        if (stream.IsConnected) stream.Disconnect();
      }
    }
  }

  string ProcessRequest(string? request)
  {
    return request switch
    {
      "ping" => "pong",
      "time" => DateTime.Now.ToString("o"),
      "status" => resultsStorage.Statuses.Values.All(s => s == System.Net.NetworkInformation.IPStatus.Success) ? "ok" : "error",
      _ => "unknown"
    };
  }
}
