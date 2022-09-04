using Newtonsoft.Json;

namespace GameLib.Plugin.BattleNet.Model;

internal class BNetGames
{
    [JsonProperty("Games")]
    public List<BNetGame> Games { get; set; } = new();
}

internal class BNetGame
{
    [JsonProperty("InternalId")]
    public string? InternalId { get; set; }

    [JsonProperty("ProductId")]
    public string? ProductId { get; set; }

    [JsonProperty("Name")]
    public string? Name { get; set; }

    [JsonProperty("Executables")]
    public List<string> Executables { get; set; } = new();
}