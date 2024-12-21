using System.Diagnostics;
using System.IO.Pipes;

namespace HekyLab.PingTray.UI;

public class TrayApplication : ApplicationContext
{
  private NotifyIcon _notifyIcon;
  private CancellationTokenSource _cts = new();

  public TrayApplication()
  {
    _notifyIcon = new NotifyIcon
    {
      Visible = true,
      Icon = SystemIcons.Application,
      Text = "Ping",
      ContextMenuStrip = new ContextMenuStrip()
    };
    _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, ExitApplication);

    Task.Run(() => MonitorStatusAsync(_cts.Token));
  }

  private async Task MonitorStatusAsync(CancellationToken token)
  {
    while (!token.IsCancellationRequested)
    {
      try
      {
        string? status = await SendStatusCommandAsync();
        UpdateTrayIcon(status);
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        Console.WriteLine(e.StackTrace);
        UpdateTrayIcon("error");
      }

      await Task.Delay(2000, token);
    }
  }

  private static async Task<string?> SendStatusCommandAsync()
  {
    using var pipeClient = new NamedPipeClientStream(".", "HekyLab.PingTray", PipeDirection.InOut, PipeOptions.Asynchronous);
    await pipeClient.ConnectAsync();
    var reader = new StreamReader(pipeClient);
    var writer = new StreamWriter(pipeClient) { AutoFlush = true };
    await Task.Delay(5000);
    await writer.WriteLineAsync("status");
    await writer.WriteLineAsync("time");
    var status = await reader.ReadLineAsync();
    Console.WriteLine(status);
    Console.WriteLine(await reader.ReadLineAsync());
    return status;
  }

  private void UpdateTrayIcon(string? status)
  {
    if (_notifyIcon == null) return;

    _notifyIcon.Icon = status == "ok"
        ? SystemIcons.Information
        : SystemIcons.Error;
  }

  private void ExitApplication(object? sender, EventArgs e)
  {
    _cts.Cancel();
    ExitThread();
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      _cts?.Cancel();
      _cts?.Dispose();
      _notifyIcon?.Dispose();
    }
    base.Dispose(disposing);
  }
}

