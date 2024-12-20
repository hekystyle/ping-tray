using System.IO.Pipes;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HekyLab.PingTray.UI;

static class Program
{
  static async Task Main(string[] args)
  {
    string pipeName = "HekyLab.PingTray";

    using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
    Console.WriteLine($"Klient se připojuje k pojmenované rouře '{pipeName}'...");
    await client.ConnectAsync();
    Console.WriteLine("Připojeno k serveru!");

    // Odeslání zprávy serveru
    using var writer = new StreamWriter(client) { AutoFlush = true };
    using var reader = new StreamReader(client);
    await writer.WriteLineAsync("ping");
    string? response1 = await reader.ReadLineAsync();
    await writer.WriteLineAsync("time");
    string? response2 = await reader.ReadLineAsync();
    Console.WriteLine("Zprávy odeslány a přečteny");

    // Čtení odpovědi od serveru
    Console.WriteLine($"Přijato: {response1} {response2}");

    Console.ReadLine();
  }
  /// <summary>
  ///  The main entry point for the application.
  /// </summary>
  // [STAThread]
  // static void Main()
  // {
  //   Test();
  //   // To customize application configuration such as set high DPI settings or default font,
  //   // see https://aka.ms/applicationconfiguration.
  //   ApplicationConfiguration.Initialize();

  //   var host = CreateHost();
  //   Application.Run(host.Services.GetRequiredService<App>());
  // }

  static IHost CreateHost()
  {
    return Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
          services.AddTransient<App>();
        }).Build();
  }
}
