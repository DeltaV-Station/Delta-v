using System.Numerics;
using Content.Server.Ghost;
using Content.Server.Humanoid;
using Content.Shared._Shitmed.Body.Part;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Timing;
using System.Numerics;
using Content.Server.Polymorph.Components; // DeltaV
using Content.Server.Polymorph.Systems; // DeltaV
using Content.Shared.Damage.Components;

// Shitmed Change
using System.Linq;
using Content.Shared.Damage;

namespace Content.Server.Body.Systems;

public sealed class BodySystem : SharedBodySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!; // Shitmed Change
    [Dependency] private readonly GhostSystem _ghostSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!; // Shitmed Change
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, MoveInputEvent>(OnRelayMoveInput);
        SubscribeLocalEvent<BodyComponent, ApplyMetabolicMultiplierEvent>(OnApplyMetabolicMultiplier);
    }

    private void OnRelayMoveInput(Entity<BodyComponent> ent, ref MoveInputEvent args)
    {
        // If they haven't actually moved then ignore it.
        if ((args.Entity.Comp.HeldMoveButtons &
             (MoveButtons.Down | MoveButtons.Left | MoveButtons.Up | MoveButtons.Right)) == 0x0)
        {
            return;
        }

        if (_mobState.IsDead(ent) && _mindSystem.TryGetMind(ent, out var mindId, out var mind))
        {
            mind.TimeOfDeath ??= _gameTiming.RealTime;
            _ghostSystem.OnGhostAttempt(mindId, canReturnGlobal: true, mind: mind);
        }
    }

    private void OnApplyMetabolicMultiplier(
        Entity<BodyComponent> ent,
        ref ApplyMetabolicMultiplierEvent args)
    {
        foreach (var organ in GetBodyOrgans(ent, ent))
        {
            RaiseLocalEvent(organ.Id, ref args);
        }
    }

    protected override void AddPart(
        Entity<BodyComponent?> bodyEnt,
        Entity<BodyPartComponent> partEnt,
        string slotId)
    {
        // TODO: Predict this probably.
        base.AddPart(bodyEnt, partEnt, slotId);

        var layer = partEnt.Comp.ToHumanoidLayers();
        if (layer != null)
        {
            var layers = HumanoidVisualLayersExtension.Sublayers(layer.Value);
            _humanoidSystem.SetLayersVisibility(bodyEnt.Owner, layers, visible: true);
        }
    }

    public override void RemovePart( // DeltaV - Made public
        Entity<BodyComponent?> bodyEnt,
        Entity<BodyPartComponent> partEnt,
        string slotId)
    {
        base.RemovePart(bodyEnt, partEnt, slotId);

        if (!TryComp<HumanoidAppearanceComponent>(bodyEnt, out var humanoid))
            return;

        var layer = partEnt.Comp.ToHumanoidLayers();

        if (layer is null)
            return;

        var layers = HumanoidVisualLayersExtension.Sublayers(layer.Value);
        _humanoidSystem.SetLayersVisibility((bodyEnt, humanoid), layers, visible: false);
        _appearance.SetData(bodyEnt, layer, true); // Shitmed Change
    }

    // Shitmed Change Start
    protected override void ApplyPartMarkings(EntityUid target, BodyPartAppearanceComponent component)
    {
        return;
    }

    protected override void RemoveBodyMarkings(EntityUid target, BodyPartAppearanceComponent partAppearance, HumanoidAppearanceComponent bodyAppearance)
    {
        foreach (var (visualLayer, markingList) in partAppearance.Markings)
            foreach (var marking in markingList)
                _humanoidSystem.RemoveMarking(target, marking.MarkingId, sync: false, humanoid: bodyAppearance);

        Dirty(target, bodyAppearance);
    }

    protected override void PartRemoveDamage(Entity<BodyComponent?> bodyEnt, Entity<BodyPartComponent> partEnt)
    {
        var bleeding = partEnt.Comp.SeverBleeding;
        if (partEnt.Comp.IsVital)
            bleeding *= 2f;
        TryComp<BloodstreamComponent>(bodyEnt, out var bloodstream);
        _bloodstream.TryModifyBleedAmount((bodyEnt, bloodstream), bleeding);
    }
    // Shitmed Change End
}
