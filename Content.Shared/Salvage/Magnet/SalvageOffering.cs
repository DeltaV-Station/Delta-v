namespace Content.Shared.Salvage.Magnet;

/// <summary>
/// Asteroid offered for the magnet.
/// </summary>
public record struct SalvageOffering : ISalvageMagnetOffering
{
    public SalvageMapPrototype SalvageMap;

    uint ISalvageMagnetOffering.Cost => 500; // DeltaV: was 1000 (from deltanedas), lowered to 500 to make it less of a grind
}
