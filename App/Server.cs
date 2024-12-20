using System.IO.Pipes;

namespace HekyLab.PingTray.App;

public class Server(ILogger<Server> logger, IPingResultsStorage resultsStorage) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    using var stream = new NamedPipeServerStream("HekyLab.PingTray", PipeDirection.InOut);

    while (!stoppingToken.IsCancellationRequested)
    {
      logger.LogInformation("Waiting for connection...");
      await stream.WaitForConnectionAsync(stoppingToken);
      logger.LogInformation("Connected");

      try
      {
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream) { AutoFlush = true };

        var request = await reader.ReadLineAsync(stoppingToken);
        logger.LogInformation("Received request: {request}", request);

        var response = ProcessRequest(request);

        logger.LogInformation("Sending response: {response}", response);
        await writer.WriteLineAsync(response);
      }
      catch (IOException ex)
      {
        logger.LogError(ex, "Error while processing request");
      }

      logger.LogInformation("Disconnecting...");
      stream.Disconnect();
      logger.LogInformation("Disconnected");
    }
  }

  string ProcessRequest(string? request)
  {
    return request switch
    {
      "ping" => "pong",
      "time" => DateTime.Now.ToString(),
      "status" => resultsStorage.Statuses.Values.All(s => s == System.Net.NetworkInformation.IPStatus.Success) ? "ok" : "error",
      _ => "unknown"
    };
  }
}
