using HekyLab.PingTray.WPF.Properties;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Windows;

namespace HekyLab.PingTray.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
  private readonly Settings settings = Settings.Default;

  private readonly MainViewModel viewModel = new (Settings.Default);
  public MainViewModel ViewModel => viewModel;

  private readonly BackgroundWorker pingWorker = new();

  private readonly NotifyIcon notifyIcon = new()
  {
    Icon = SystemIcons.Question,
    Visible = true,
    Text = "Ping Tray",
  };

  public MainWindow()
  {
    InitializeComponent();

    pingWorker.DoWork += PingWorker_DoWork;
    pingWorker.RunWorkerAsync();
  }

  protected override void OnClosing(CancelEventArgs e)
  {
    base.OnClosing(e);
    notifyIcon.Visible = false; // hack for "tray icon not disappearing" bug
  }

  private void UpdateTrayIcon()
  {
    notifyIcon.Icon = ViewModel.IsEveryHostHealthy switch
    {
      true => SystemIcons.Information,
      false => SystemIcons.Error,
    };
  }

  private async void PingWorker_DoWork(object? sender, DoWorkEventArgs e)
  {
    if (sender is not BackgroundWorker worker) return;

    while (!worker.CancellationPending)
    {
      await ViewModel.PingAsync();
      UpdateTrayIcon();
      await Task.Delay(settings.DelayBetweenPings);
    }
  }
}
