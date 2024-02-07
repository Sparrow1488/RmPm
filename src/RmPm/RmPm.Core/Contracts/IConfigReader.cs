namespace RmPm.Core.Contracts;

public interface IJsonReader<T>
{
    string ToJson(T read);
    T? ToConfig(string json);
}

public interface IBase64Reader<in T>
{
    string ToBase64(T read, string? tag = null);
}

public interface IConfigReader<TConfig> : IJsonReader<TConfig>, IBase64Reader<TConfig>
{
    
}