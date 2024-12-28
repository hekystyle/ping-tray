using HekyLab.PingTray.Common;
using System.IO.Pipes;
using System.Text.Json;

namespace HekyLab.PingTray.UI;

public class Client()
{
  private static NamedPipeClientStream CreateStream() => new(".", "HekyLab.PingTray", PipeDirection.InOut, PipeOptions.Asynchronous);

  private static async Task<string?> SendRequest(string request)
  {
    using var stream = CreateStream();
    await stream.ConnectAsync();
    var reader = new StreamReader(stream);
    var writer = new StreamWriter(stream) { AutoFlush = true };
    await writer.WriteLineAsync(request);
    return await reader.ReadLineAsync();
  }

  public async Task<string?> GetStatus()
  {
    return await SendRequest("status");
  }

  public async Task<ResultsResponse> GetResults()
  {
    var response = await SendRequest("results") ?? "";
    return JsonSerializer.Deserialize<ResultsResponse>(response);
  }
}
