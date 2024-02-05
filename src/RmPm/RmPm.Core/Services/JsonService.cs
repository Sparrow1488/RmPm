using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RmPm.Core.Contracts;

namespace RmPm.Core.Services;

public class JsonService : IJsonService
{
    private readonly JsonSerializerSettings _serializeSettings;

    public JsonService()
    {
        _serializeSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            Error = (_,err) => err.ErrorContext.Handled = true
        };
    }
    
    public string Serialize<T>(T obj)
    {
        return JsonConvert.SerializeObject(obj, _serializeSettings);
    }

    public T? Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, _serializeSettings);
    }
}