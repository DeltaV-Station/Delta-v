using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Species;

public sealed partial class SpeciesHiderSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private static Dictionary<string, bool>? _hiddenSpecies;

    public override void Initialize()
    {
        base.Initialize();
        if (_hiddenSpecies != null) return; // Prevent duplicate initialization, as this is a static list.
        _hiddenSpecies = [];
        foreach (var species in _proto.EnumeratePrototypes<SpeciesPrototype>())
        {
            Log.Info($"Creating CVAR: species.hide.{species.ID}");
            var cvar = CVarDef.Create($"species.hide.{species.ID}", species.DefaultHidden, CVar.SERVER | CVar.REPLICATED);
            _cfg.RegisterCVar(cvar.Name, species.DefaultHidden, CVar.SERVER | CVar.REPLICATED);
            Subs.CVar(_cfg, cvar, val => _hiddenSpecies[species.ID] = val, true);
        }
    }

    public static bool IsHidden(string speciesId)
    {
        // If _hiddenSpecies is somehow not yet initialized, probably an invalid state.
        if (_hiddenSpecies is null) return false;
        return _hiddenSpecies.ContainsKey(speciesId) && _hiddenSpecies[speciesId];
    }
}