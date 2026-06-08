namespace Content.DiscordBot;

public sealed class Config
{
    public string Token { get; set; } = string.Empty;

    public string DatabaseString { get; set; } = string.Empty;

    public string DatabaseContext { get; set; } = "postgres";

    /// <summary>
    /// Discord "guild" ID (which is just the server ID). Default is DeltaV's server ID.
    /// </summary>
    public ulong Guild { get; set; } = 1513310351490289834UL;
}
