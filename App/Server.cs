using System.Globalization;
using System.IO.Pipes;

namespace HekyLab.PingTray.App;

public class Server(ILogger<Server> logger, IPingResultsStorage resultsStorage) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      logger.LogInformation("");
      using var stream = new NamedPipeServerStream("HekyLab.PingTray", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

      logger.LogInformation("Waiting for connection...");
      await stream.WaitForConnectionAsync(stoppingToken);
      logger.LogInformation("Connected");
      try
      {
        var reader = new StreamReader(stream);
        var writer = new StreamWriter(stream) { AutoFlush = true };

        while (!stoppingToken.IsCancellationRequested && !reader.EndOfStream)
        {
          logger.LogInformation("Reading line...");
          var request = await reader.ReadLineAsync(stoppingToken);
          logger.LogInformation("Received request: {request}", request);

          var response = ProcessRequest(request);

          logger.LogInformation("Sending response: {response}", response);
          await writer.WriteLineAsync(response);
          logger.LogInformation("Response sent");
        }
      }
      catch (IOException ex)
      {
        logger.LogError(ex, "Error while processing request");
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
