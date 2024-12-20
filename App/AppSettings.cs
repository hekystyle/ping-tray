namespace HekyLab.PingTray.App;

public class AppSettings
{
  public required IEnumerable<string> Hosts { get; set; }
  public int PingDelay { get; set; } = 5000;
}
