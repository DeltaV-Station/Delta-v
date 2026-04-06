using Content.Server.Cloning;
using Content.Server.Mind;
using Content.Server.Station.Systems;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;
using Content.Shared._DV.Species;
using Content.Shared.Bed.Sleep;
using Content.Shared.Body.Components;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Preferences;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Robust.Server.Player;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._DV.Psionics.Systems.PsionicPowers;

public sealed class FracturedFormPowerSystem : SharedFracturedFormPowerSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly CloningSystem _cloning = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PvsOverrideSystem _pvsOverride = default!;

    // holy initialize performance? but better for it to happen once than the double dict lookup every tick!!
    private EntityQuery<FracturedFormBodyComponent> _bodyQuery;
    private EntityQuery<SleepingComponent> _sleepingQuery;

    public override void Initialize()
    {
        base.Initialize();

        _bodyQuery = GetEntityQuery<FracturedFormBodyComponent>();
        _sleepingQuery = GetEntityQuery<SleepingComponent>();
    }

    protected override void OnPowerInit(Entity<FracturedFormPowerComponent> power, ref MapInitEvent args)
    {
        base.OnPowerInit(power, ref args);

        // The next random swap is between 5 and 20 minutes.
        var randomTime = Random.Next(power.Comp.NextSwapMinTime, power.Comp.NextSwapMaxTime);
        power.Comp.NextSwap = Timing.CurTime + randomTime;
        power.Comp.NextVoluntarySwap = Timing.CurTime + power.Comp.VoluntarySwapCooldown;

        // Don't generate a new body if we're already part of a network.
        if (HasComp<FracturedFormBodyComponent>(power))
            return;

        // Don't make bodies if there is no body. This is solely for test fails.
        if (!HasComp<BodyComponent>(power))
            return;

        var bodyComp = AddComp<FracturedFormBodyComponent>(power);
        bodyComp.ControllingForm = power.Owner;
        power.Comp.Bodies.Add(power);
        var body = GenerateForm(power);
        // hide the SSD indicator.
        if (SsdQuery.TryComp(body, out var ssdComp))
            ssdComp.IsSSD = false;
    }

    private EntityUid GenerateForm(Entity<FracturedFormPowerComponent> original)
    {
        // Form:
        //  - Same appearance as original
        //  - Different apperance, still humanoid
        // Equipment:
        //  - Same as original body
        //  - Nude and helpless

        var xform = Transform(original);

        var hasGear = Random.Prob(original.Comp.HasGearChance);

        if (Random.Prob(original.Comp.DifferentSpeciesChance) || !_cloning.TryCloning(original, _transform.GetMapCoordinates(original), hasGear ? original.Comp.CopyClothed : original.Comp.CopyNaked, out var newBody)) // Slightly lower chance to copy the original body
        {
            // Either the dice rolled poorly, or the cloning failed. Either way, make a new body instead. (Or try to)
            var validSpecies = new List<ProtoId<SpeciesPrototype>>();
            var speciesPrototypes = _prototype.EnumeratePrototypes<SpeciesPrototype>();
            foreach (var proto in speciesPrototypes)
            {
                var speciesEntityPrototype = _prototype.Index<EntityPrototype>(proto.Prototype);
                // If they have the PotentialPsionicComponent, they can be psionic.
                if (proto.RoundStart && speciesEntityPrototype.TryGetComponent<PotentialPsionicComponent>(out _, Factory) && !SpeciesHiderSystem.IsHidden(proto.ID))
                    validSpecies.Add(proto.ID);
            }
            var species = Random.Pick(validSpecies);
            var character = HumanoidCharacterProfile.RandomWithSpecies(species);
            newBody = _stationSpawning.SpawnPlayerMob(xform.Coordinates, hasGear ? original.Comp.VisitorJob : original.Comp.NakedJob, character, _station.GetOwningStation(original.Owner));
            if (newBody is not { } bodyV || Deleted(bodyV))
            {
                Log.Error($"Failed to create a new body for {ToPrettyString(original)}. This is a bug.");
                return EntityUid.Invalid;
            }
        }

        if (newBody is not { } body || Deleted(body))
            return default!;

        var bodyComp = AddComp<FracturedFormBodyComponent>(body);
        original.Comp.Bodies.Add(body);
        bodyComp.ControllingForm = original.Owner;

        if (_player.TryGetSessionByEntity(original, out var session))
            _pvsOverride.AddSessionOverride(body, session);

        Dirty(original);

        return body;
    }

    private bool TryGetValidBody(Entity<FracturedFormPowerComponent> psionic, out EntityUid validBody)
    {
        foreach (var body in psionic.Comp.Bodies)
        {
            if (!IsValidBody(psionic, body))
                continue;

            validBody = body;
            return true;
        }
        validBody = default;
        return false;
    }

    private void Swap(Entity<FracturedFormPowerComponent> psionic)
    {
        if (!TryGetValidBody(psionic, out var targetBody))
            return;

        _audio.PlayPvs(psionic.Comp.SwapSound, psionic);
        // Transfer mind if present
        if (MindContainerQuery.TryComp(psionic, out var mindContainer) && mindContainer.Mind.HasValue)
            _mind.TransferTo(mindContainer.Mind.Value, targetBody);
        // Wake up the new body
        Sleeping.TryWaking(targetBody);
        // Remove the action.
        Action.RemoveAction(psionic.Comp.ActionEntity);
        // Create new component on target and copy data
        var duplicate = EnsureComp<FracturedFormPowerComponent>(targetBody);
        duplicate.Bodies = psionic.Comp.Bodies;
        // Update all body references
        foreach (var body in duplicate.Bodies)
        {
            if (_bodyQuery.TryComp(body, out var bodyComp))
                bodyComp.ControllingForm = targetBody;
        }

        if (_player.TryGetSessionByEntity(targetBody, out var session))
        {
            _pvsOverride.AddSessionOverride(psionic, session);
            _pvsOverride.RemoveSessionOverride(targetBody, session);
        }

        RemCompDeferred<FracturedFormPowerComponent>(psionic);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        List<Entity<FracturedFormPowerComponent>> swapTargets = [];

        var entities = EntityQueryEnumerator<FracturedFormPowerComponent, MobStateComponent>();
        while (entities.MoveNext(out var uid, out var comp, out var mobState))
        {
            // Check sleep warning
            if (!comp.SleepWarned && Timing.CurTime > comp.NextSwap - comp.WarningTimeBeforeSleep)
            {
                comp.SleepWarned = true;
                Popup.PopupEntity(Loc.GetString("psionic-power-fractured-form-sleepy"), uid, uid, PopupType.LargeCaution);
                Chat.TryEmoteWithChat(uid, "Yawn");
            }
            // Swap check
            if ((_sleepingQuery.HasComp(uid) || MobState.IsIncapacitated(uid, mobState)) && Timing.CurTime > comp.NextVoluntarySwap
                || Timing.CurTime > comp.NextSwap)
                swapTargets.Add((uid, comp));
        }

        foreach (var target in swapTargets)
        {
            Swap(target);
        }

        // Process bodies
        var bodies = EntityQueryEnumerator<FracturedFormBodyComponent, MobStateComponent>();
        while (bodies.MoveNext(out var uid, out var comp, out var mobState))
        {
            // Put to sleep if no sleeping component and no mind
            if (!_sleepingQuery.HasComp(uid) && !_mind.GetMind(uid).HasValue && !FracturedQuery.HasComp(uid))
                Sleeping.TrySleeping((uid, mobState));
            // Cleanup invalid bodies
            if (!comp.ControllingForm.IsValid()
                || Deleted(comp.ControllingForm)
                || !FracturedQuery.HasComp(comp.ControllingForm))
            {
                RemCompDeferred<FracturedFormBodyComponent>(uid);
            }
        }
    }
}
