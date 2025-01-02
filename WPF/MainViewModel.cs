using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HekyLab.PingTray.WPF.Properties;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;

namespace HekyLab.PingTray.WPF;

public class HostPingResult : ObservableObject
{
  public const string Idle = "Idle";

  public required string Host { get; init; } = string.Empty;

  private DateTime? _lastPingAt = null;
  public DateTime? LastPingAt => _lastPingAt;

  private readonly ConcurrentQueue<IPStatus> _pingHistory = new();
  public string Status => _pingHistory.IsEmpty
      ? Idle
      : _pingHistory.Take(3).GroupBy(s => s).OrderByDescending(g => g.Count()).First().Key.ToString();

  public void AddPingResult(IPStatus status)
  {
    OnPropertyChanging(nameof(Status));
    _pingHistory.Enqueue(status);
    if (_pingHistory.Count > 3) _pingHistory.TryDequeue(out _);
    OnPropertyChanged(nameof(Status));
    SetProperty(ref _lastPingAt, DateTime.Now, nameof(LastPingAt));
  }
}

public partial class MainViewModel : ObservableObject
{
  private readonly Settings settings;
  public MainViewModel(Settings settings)
  {
    this.settings = settings;
    pingResults = new(settings.Hosts.Cast<string>().Select(host => new HostPingResult { Host = host }));
    pingResults.CollectionChanged += PingResults_CollectionChanged;
  }

  private readonly ObservableCollection<HostPingResult> pingResults;
  public ObservableCollection<HostPingResult> PingResults => pingResults;

  public bool IsEveryHostHealthy => PingResults.All(p => p.Status == HostPingResult.Idle || p.Status == IPStatus.Success.ToString());

  [ObservableProperty]
  [NotifyCanExecuteChangedFor(nameof(AddHostCommand))]
  private string? newHost;

  [ObservableProperty]
  [NotifyCanExecuteChangedFor(nameof(RemoveHostCommand))]
  private HostPingResult? selectedPingResult;

  [RelayCommand(CanExecute = nameof(CanAddHost))]
  private void AddHost(string? host)
  {
    if (host is null) return;
    PingResults.Add(new HostPingResult { Host = host });
    NewHost = string.Empty;
  }
  private static bool CanAddHost(string? host) => !string.IsNullOrWhiteSpace(host);

  [RelayCommand(CanExecute = nameof(CanRemoveHost))]
  private void RemoveHost(HostPingResult? pingResult)
  {
    if (pingResult is null) return;
    PingResults.Remove(pingResult);
  }
  private static bool CanRemoveHost(HostPingResult? pingResult) => pingResult is not null;

  public async Task PingAsync()
  {
    var tasks = PingResults.Select(static async result =>
    {
      using var ping = new Ping();
      var reply = await ping.SendPingAsync(result.Host);
      result.AddPingResult(reply.Status);
    });
    await Task.WhenAll(tasks);
  }

  private void PingResults_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
  {
    switch (e.Action)
    {
      case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
        settings.Hosts.AddRange(e.NewItems?.Cast<HostPingResult>().Select(s => s.Host).ToArray() ?? []);
        settings.Save();
        break;
      case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
        foreach (var item in e.OldItems?.Cast<HostPingResult>().Select(s => s.Host) ?? [])
        {
          settings.Hosts.Remove(item);
        }
        settings.Save();
        break;
      default:
        throw new NotImplementedException($"Unsupported action {e.Action}");
    }
  }
}
