using System.Collections.Concurrent;
using System.Net.NetworkInformation;

namespace HekyLab.PingTray.App;

public interface IPingResultsStorage
{
  IReadOnlyDictionary<string, IPStatus> Statuses { get; }
  void Store(string host, IPStatus status);
}

public class MemoryPingResultsStorage : IPingResultsStorage
{
  private ConcurrentDictionary<string, IPStatus> _statuses = [];

  public IReadOnlyDictionary<string, IPStatus> Statuses => _statuses;

  public void Store(string host, IPStatus status)
  {
    _statuses[host] = status;
  }
}