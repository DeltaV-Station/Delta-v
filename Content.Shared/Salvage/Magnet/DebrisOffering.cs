using Content.Shared.Procedural;    //DeltaV

namespace Content.Shared.Salvage.Magnet;

/// <summary>
/// Space debis offered for the magnet.
/// </summary>
public record struct DebrisOffering : ISalvageMagnetOffering
{
    public string Id;
    public DungeonConfig DungeonConfig; //DeltaV - store config for dynamic loot assignment.
    public string LootId; //DeltaV - store loot ID for dynamic assignment.
}
