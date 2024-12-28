using HekyLab.PingTray.WPF.Properties;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;

namespace HekyLab.PingTray.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
  private readonly Settings settings = Settings.Default;
  private readonly MainViewModel _viewModel = new(Settings.Default.Hosts.Cast<string>());
  public MainViewModel ViewModel => _viewModel;

  private readonly BackgroundWorker _pingWorker = new();

  public MainWindow()
  {
    InitializeComponent();

    _pingWorker.DoWork += pingWorker_DoWork;
    _pingWorker.RunWorkerAsync();

    ViewModel.PingResults.CollectionChanged += PingResults_CollectionChanged;
  }

  private void PingResults_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
  {
    switch (e.Action)
    {
      case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
        settings.Hosts.AddRange(e.NewItems.Cast<HostPingResult>().Select(s => s.Host).ToArray());
        settings.Save();
        break;
      case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
        foreach (var item in e.OldItems.Cast<HostPingResult>().Select(s => s.Host))
        {
          settings.Hosts.Remove(item);
        }
        settings.Save();
        break;
      default:
        throw new NotImplementedException($"Unsupported action {e.Action}");
    }
  }

  private async void pingWorker_DoWork(object? sender, DoWorkEventArgs e)
  {
    if (sender is not BackgroundWorker worker) return;

    while (!worker.CancellationPending)
    {
      var tasks = _viewModel.PingResults.Select(static async result =>
      {
        using var ping = new Ping();
        var reply = await ping.SendPingAsync(result.Host);
        result.AddPingResult(reply.Status);
      });
      await Task.WhenAll(tasks);
      await Task.Delay(Settings.Default.DelayBetweenPings);
    }
  }

  private void command_addHost_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = !string.IsNullOrWhiteSpace(_viewModel.NewHost) && !Validation.GetHasError(textBox_newHost);
  }

  private void command_addHost_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
  {
    _viewModel.PingResults.Add(new HostPingResult { Host = _viewModel.NewHost });
    _viewModel.NewHost = string.Empty;
    textBox_newHostNote.Text = string.Empty;
  }

  private void command_deleteHost_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = listView_pingResults.SelectionMode == SelectionMode.Single
      ? listView_pingResults.SelectedItem is not null
      : listView_pingResults.SelectedItems.Count > 0;
  }

  private void command_deleteHost_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
  {
    if (e.Source is ListView listView)
    {
      if (listView.SelectionMode == SelectionMode.Single && listView.SelectedItem is HostPingResult result)
      {
        _viewModel.PingResults.Remove(result);
      }
      else
      {
        foreach (var item in listView.SelectedItems.Cast<HostPingResult>())
        {
          _viewModel.PingResults.Remove(item);
        }
      }
    }
  }
}
