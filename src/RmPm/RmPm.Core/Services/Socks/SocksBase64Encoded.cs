using System.Text;
using RmPm.Core.Configuration;

namespace RmPm.Core.Services.Socks;

public readonly struct SocksBase64Encoded
{
    private readonly SocksConfig _config;
    private readonly string? _tag;

    public SocksBase64Encoded(SocksConfig config, string? tag = null)
    {
        _config = config;
        _tag = tag;
    }

    public override string ToString()
    {
        // ss://method:password@hostname:port
        // ss://BASE64-ENCODED-STRING-WITHOUT-PADDING#TAG

        var en = Encoding.UTF8.GetBytes($"{_config.Method}:{_config.Password}@{_config.Server}:{_config.ServerPort}");
        return "ss://" + Convert.ToBase64String(en) + (string.IsNullOrWhiteSpace(_tag) ? "" : "#" + _tag);
    }
}