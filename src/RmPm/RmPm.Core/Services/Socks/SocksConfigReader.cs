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
    
    public string ReadJson(SocksConfig read)
    {
        return _jsonService.Serialize(read);
    }

    public SocksConfig? ReadBack(string json)
    {
        return _jsonService.Deserialize<SocksConfig>(json);
    }

    public string ReadBase64(SocksConfig read, string? tag = null)
    {
        return new SocksBase64Encoded(read, tag).ToString();
    }
}