using Content.Server._Funkystation.Atmos.Components;
using Robust.Shared.Spawners;
using Content.Shared.Atmos;
using Content.Server.Atmos.EntitySystems;
using System.Linq;

namespace Content.Server._Funkystation.Atmos.EntitySystems;


/// <summary>
/// ATMOS - Extinguisher Nozzle
/// Sets atmospheric temperature to 20C and removes all toxins. 
/// </summary>
public sealed class AtmosResinDespawnSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmo = default!;
    [Dependency] private readonly GasTileOverlaySystem _gasOverlaySystem = default!;
    
    private Gas[] _gasesToRemove = Array.Empty<Gas>();

    public override void Initialize()
    {
        base.Initialize();

        _gasesToRemove = Enum.GetValues(typeof(Gas))
            .Cast<Gas>()
            .Where(gas => gas != Gas.Oxygen && gas != Gas.Nitrogen)
            .ToArray();

        SubscribeLocalEvent<AtmosResinDespawnComponent, TimedDespawnEvent>(OnDespawn);
    }

    private void OnDespawn(EntityUid uid, AtmosResinDespawnComponent comp, ref TimedDespawnEvent args)
    {
        if (!TryComp(uid, out TransformComponent? xform))
            return;

        var mix = _atmo.GetContainingMixture(uid, true);
        GasMixture tempMix = new();
        if (mix is null) return;

        float totalMolesRemoved = 0f;

        foreach (var gas in _gasesToRemove)
        {
            float moles = mix.GetMoles(gas);
            if (moles > 0)
            {
                totalMolesRemoved += moles;
                mix.AdjustMoles(gas, -moles);
            }
        }

        if (totalMolesRemoved > 0)
        {
            tempMix.AdjustMoles(Gas.Oxygen, totalMolesRemoved * 0.3f);
            tempMix.AdjustMoles(Gas.Nitrogen, totalMolesRemoved * 0.7f);
        }

        _atmo.Merge(mix, tempMix);
        mix.Temperature = Atmospherics.T20C;
        _gasOverlaySystem.UpdateSessions();
    }
}