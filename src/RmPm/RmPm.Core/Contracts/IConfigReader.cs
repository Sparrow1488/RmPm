namespace RmPm.Core.Contracts;

public interface IJsonReader<T>
{
    string ReadJson(T read);
    T? ReadBack(string json);
}

public interface IBase64Reader<T>
{
    string ReadBase64(T read, string? tag = null);
}

public interface IConfigReader<TConfig> : IJsonReader<TConfig>, IBase64Reader<TConfig>
{
    
}