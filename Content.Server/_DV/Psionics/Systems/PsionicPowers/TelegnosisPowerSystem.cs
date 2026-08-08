using System.Numerics;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Systems;
using Content.Server.Disposal.Unit;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;
using Content.Shared.Atmos;
using Content.Shared.Body.Components;
using Content.Shared.Mech.Components;
using Content.Shared.Medical.Cryogenics;
using Content.Shared.Storage.Components;

namespace Content.Server._DV.Psionics.Systems.PsionicPowers;

public sealed class TelegnosisPowerSystem : SharedTelegnosisPowerSystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly PsionicSystem _psionic = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TelegnosisPowerComponent, InhaleLocationEvent>(OnInhaleLocation, after: [typeof(InsideCryoPodComponent), typeof(InternalsComponent), typeof(BeingDisposedComponent), typeof(InsideEntityStorageComponent), typeof(MechPilotComponent)]);
    }

    // This needs to be here on serverside, because mindswap CANNOT be predicted.
    // The logic for transferring minds is server-side only. If we don't put this here, it'll look bad for the person.
    protected override void OnPowerUsed(Entity<TelegnosisPowerComponent> psionic, ref TelegnosisPowerActionEvent args)
    {
        var projection = Spawn(psionic.Comp.Prototype, Transform(psionic).Coordinates);

        _transform.AttachToGridOrMap(projection);
        if (!_psionic.SwapMinds(args.Performer, projection))
        {
            // If swap didn't work out, delete the spawned projection.
            QueueDel(projection);
            return;
        }

        AfterPowerUsed(psionic, args.Performer);
    }

    private void OnInhaleLocation(Entity<TelegnosisPowerComponent> entity, ref InhaleLocationEvent args)
    {
        var sensorUid = GetCasterProjection(entity);
        if (sensorUid == default)
            return;
        // Determine the distance to the sensor, this will be used to dilute the amount of air we take in.
        var sensorPosition = _transform.GetWorldPosition(sensorUid);
        var projectionPosition = _transform.GetWorldPosition(entity);
        // A linear curve from 1.0 at 7 tiles away, to 0 at 57 tiles away
        var distance = Vector2.Distance(sensorPosition, projectionPosition);
        float gasMult = Math.Clamp(1f - (distance - 7f) / 50f, 0f, 1f);
        args.Gas = (args.Gas ?? _atmos.GetContainingMixture(entity.Owner, excite: true))?.RemoveVolume(Atmospherics.BreathVolume * gasMult);
        if (args.Gas == null)
            return;
        args.Gas.Volume = Math.Min(args.Gas.Volume, Atmospherics.BreathVolume);
    }
}
