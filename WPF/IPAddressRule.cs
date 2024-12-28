using System.Globalization;
using System.Net;
using System.Windows.Controls;

namespace HekyLab.PingTray.WPF;

public class IPAddressRule : ValidationRule
{
  public override ValidationResult Validate(object value, CultureInfo cultureInfo)
  {
    if (IPAddress.TryParse((string)value, out _)) return ValidationResult.ValidResult;

    return new ValidationResult(false, "Invalid IP Address");
  }
}
