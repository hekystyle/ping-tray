using Microsoft.Extensions.Logging;

namespace HekyLab.PingTray.UI;

static class Program
{
  [STAThread]
  static void Main()
  {
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
    Application.Run(new TrayApplication(factory.CreateLogger<TrayApplication>()));
  }
}
