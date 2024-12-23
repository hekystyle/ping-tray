using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HekyLab.PingTray.UI;

public class TrayApplication : ApplicationContext
{
  private readonly NotifyIcon _notifyIcon;
  private readonly CancellationTokenSource _cts = new();

  private readonly Client client = new();

  private readonly ILogger<TrayApplication> _logger;

  public TrayApplication(ILogger<TrayApplication> logger)
  {
    _logger = logger;

    _notifyIcon = new NotifyIcon
    {
      Visible = true,
      Icon = SystemIcons.Application,
      Text = "Ping",
      ContextMenuStrip = new ContextMenuStrip()
    };
    _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, ExitApplication);
    _notifyIcon.Click += NotifyIcon_Click;

    Task.Run(() => MonitorStatusAsync(_cts.Token));
  }

  private async void NotifyIcon_Click(object? sender, EventArgs e)
  {
    if (sender is null) return;
    var icon = (NotifyIcon)sender;
    icon.Text = "Loading...";
    try
    {
      var results = await client.GetResults();
      var table = string.Join(Environment.NewLine, results.Data.Select(kv => $"{kv.Key}: {kv.Value}"));
      icon.BalloonTipText = table;
      icon.ShowBalloonTip(5000);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error while getting results");
      icon.Text = "Failed to load status";
    }
  }

  private async Task MonitorStatusAsync(CancellationToken token)
  {
    while (!token.IsCancellationRequested)
    {
      try
      {
        string? status = await client.GetStatus();
        UpdateTrayIcon(status);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error while getting status");
        UpdateTrayIcon(null);
      }

      await Task.Delay(2000, token);
    }
  }

  private void UpdateTrayIcon(string? status)
  {
    if (_notifyIcon == null) return;

    _notifyIcon.Icon = ChooseIcon(status);
  }

  private static Icon ChooseIcon(string? status)
  {
    return status switch
    {
      "ok" => SystemIcons.Information,
      "error" => SystemIcons.Error,
      _ => SystemIcons.Question
    };
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

