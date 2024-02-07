using RmPm.Core.Configuration;
using RmPm.Core.Contracts;

namespace RmPm.Core.Services.Socks;

public class SocksConfigReader : IConfigReader<SocksConfig>
{
    private readonly IJsonService _jsonService;

    public SocksConfigReader(IJsonService jsonService)
    {
        _jsonService = jsonService;
    }
    
    public string ToJson(SocksConfig read)
    {
        return _jsonService.Serialize(read);
    }

    public SocksConfig? ToConfig(string json)
    {
        return _jsonService.Deserialize<SocksConfig>(json);
    }

    public string ToBase64(SocksConfig read, string? tag = null)
    {
        return new SocksBase64Encoded(read, tag).ToString();
    }
}