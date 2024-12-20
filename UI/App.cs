
using System.ComponentModel;
using System.IO.Pipes;
using System.Text;
using Microsoft.Extensions.Logging;

namespace HekyLab.PingTray.UI;

public class App : ApplicationContext
{
  private readonly System.Windows.Forms.Timer timer;
  private readonly NamedPipeClientStream pipeClient;

  public App()
  {
    // Inicializace NamedPipeClientStream
    pipeClient = new NamedPipeClientStream(".", "HekyLab.PingTray", PipeDirection.InOut, PipeOptions.Asynchronous);

    try
    {
      pipeClient.Connect(5000); // Timeout 5 sekund
    }
    catch (TimeoutException)
    {
      MessageBox.Show("Nepodařilo se připojit k Named Pipe.");
      ExitThread();
      return;
    }

    // Nastavení periodického Timeru
    timer = new System.Windows.Forms.Timer { Interval = 1000 }; // 1 sekunda
    timer.Tick += Timer_Tick;
    timer.Start();
  }

  private async void Timer_Tick(object sender, EventArgs e)
  {
    try
    {
      // Zápis do Named Pipe
      if (pipeClient.IsConnected)
      {
        byte[] writeBuffer = Encoding.UTF8.GetBytes("Hello from client\n");
        await pipeClient.WriteAsync(writeBuffer, 0, writeBuffer.Length);

        // Čtení z Named Pipe
        byte[] readBuffer = new byte[1024];
        int bytesRead = await pipeClient.ReadAsync(readBuffer, 0, readBuffer.Length);

        if (bytesRead > 0)
        {
          string response = Encoding.UTF8.GetString(readBuffer, 0, bytesRead);
          MessageBox.Show($"Přijato: {response}");
        }
      }
    }
    catch (IOException ex)
    {
      MessageBox.Show($"Chyba při komunikaci: {ex.Message}");
    }
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      timer?.Dispose();
      pipeClient?.Dispose();
    }
    base.Dispose(disposing);
  }
}