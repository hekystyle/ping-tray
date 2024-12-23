
using System.Net.NetworkInformation;

namespace HekyLab.PingTray.Common;

public record Response<T, M>(T Data, M Meta);

public record ResultsResponse : Response<IReadOnlyDictionary<string, IPStatus>, object>
{
  public ResultsResponse(IReadOnlyDictionary<string, IPStatus> Data, object Meta) : base(Data, Meta)
  {
  }
}
