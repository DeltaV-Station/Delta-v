using Content.Shared._DV.CCVars;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Shared._DV.Species;

public sealed partial class SpeciesHiderSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private static HashSet<string>? _hiddenSpecies = [];

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_cfg, DCCVars.HiddenSpecies, val => _hiddenSpecies = new(val.Split(",").Select(c => c.Trim())), true);
    }

    public static bool IsHidden(string speciesId)
    {
        // If _hiddenSpecies is somehow not yet initialized, probably an invalid state.
        if (_hiddenSpecies is null) return false;
        return _hiddenSpecies.Contains(speciesId);
    }
}
