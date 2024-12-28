using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.NetworkInformation;

namespace HekyLab.PingTray.WPF;

public class HostPingResult : INotifyPropertyChanged
{
  private readonly ConcurrentQueue<IPStatus> _pingHistory = new();
  private DateTime? _lastPingAt;

  public required string Host { get; init; } = string.Empty;
  public DateTime? LastPingAt => _lastPingAt;
  public string Status => _pingHistory.IsEmpty
      ? "Idle"
      : _pingHistory.Take(3).GroupBy(s => s).OrderByDescending(g => g.Count()).First().Key.ToString();

  public void AddPingResult(IPStatus status)
  {
    _pingHistory.Enqueue(status);
    if (_pingHistory.Count > 3) _pingHistory.TryDequeue(out _);
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
    _lastPingAt = DateTime.Now;
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastPingAt)));
  }
  public event PropertyChangedEventHandler? PropertyChanged;
}

public class MainViewModel(IEnumerable<string> initialHosts) : INotifyPropertyChanged
{
  private readonly ObservableCollection<HostPingResult> _pingResults = new(initialHosts.Select(host => new HostPingResult { Host = host }));
  public ObservableCollection<HostPingResult> PingResults => _pingResults;

  private string _newHost = string.Empty;

  public string NewHost
  {
    get => _newHost;
    set { _newHost = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewHost))); }
  }

  public event PropertyChangedEventHandler? PropertyChanged;
}
