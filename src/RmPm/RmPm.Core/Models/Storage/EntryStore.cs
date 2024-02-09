using Newtonsoft.Json;

namespace RmPm.Core.Models.Storage;

public class EntryStore
{
    [JsonProperty("id")] 
    public Guid Id { get; set; } = Guid.Empty;
    [JsonProperty("friendly_name")]
    public string? FriendlyName { get; set; }
    [JsonProperty("config_path")]
    public string? ConfigPath { get; set; }

    public EntryStore SetFriendlyNameByFilename(string? filename)
    {
        FriendlyName = Path.GetFileNameWithoutExtension(filename);
        return this;
    }
}