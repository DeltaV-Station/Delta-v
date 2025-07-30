using Content.Shared.Actions;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Popups;
using Content.Shared.Actions.Events;
using Content.Shared._DV.Abilities.Psionics;
using Robust.Shared.Random;
using Content.Server.Cloning;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Content.Server.Station.Systems;
using System.Linq;
using Content.Shared.Mind.Components;
using Content.Server.Mind;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Bed.Sleep;
using Robust.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Content.Server.Chat.Systems;

namespace Content.Server._DV.Abilities.Psionics;

public sealed class FracturedFormPowerSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly CloningSystem _cloning = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SleepingSystem _sleeping = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FracturedFormPowerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<FracturedFormPowerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FracturedFormPowerComponent, FracturedFormPowerActionEvent>(OnPowerUsed);
    }

    private void OnInit(Entity<FracturedFormPowerComponent> entity, ref ComponentInit args)
    {
        var component = entity.Comp;
        _actions.AddAction(entity, ref component.FracturedFormActionEntity, component.FracturedFormActionId);
        _actions.TryGetActionData(component.FracturedFormActionEntity, out var actionData);
        if (actionData is { UseDelay: not null })
            _actions.StartUseDelay(component.FracturedFormActionEntity);
        if (TryComp<PsionicComponent>(entity, out var psionic) && psionic.PsionicAbility == null)
        {
            psionic.PsionicAbility = component.FracturedFormActionEntity;
            psionic.ActivePowers.Add(component);
        }

        // Next random swap is between 5 to 20 minutes.
        component.NextSwap = _timing.CurTime + TimeSpan.FromSeconds(_random.Next(300, 1200));

        if (HasComp<FracturedFormBodyComponent>(entity)) return; // Don't generate a new body if we're already part of a network.
        AddComp<FracturedFormBodyComponent>(entity);
        component.Bodies.Add(entity);
        GenerateForm(entity);
    }

    private void OnShutdown(Entity<FracturedFormPowerComponent> entity, ref ComponentShutdown args)
    {
        _actions.RemoveAction(entity, entity.Comp.FracturedFormActionEntity);

        if (TryComp<PsionicComponent>(entity, out var psionic))
        {
            psionic.ActivePowers.Remove(entity.Comp);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Loop through fracturedForm havers, if they pass the NextSwap threshold, or they're asleep, force swap them.
        var ents = EntityQueryEnumerator<FracturedFormPowerComponent>();
        var t = _timing.CurTime;
        List<Entity<FracturedFormPowerComponent>> toSwap = new();
        while (ents.MoveNext(out var uid, out var comp))
        {
            if (!comp.SleepWarned && t > comp.NextSwap - TimeSpan.FromSeconds(5))
            {
                comp.SleepWarned = true;
                _popups.PopupEntity(Loc.GetString("fractured-form-sleepy"), uid, uid, PopupType.LargeCaution);
                _chatSystem.TryEmoteWithChat(uid, "Yawn", ChatTransmitRange.Normal);
            }
            if (HasComp<SleepingComponent>(uid) || t > comp.NextSwap)
            {
                toSwap.Add(new(uid, comp));
            }
        }
        foreach (var ent in toSwap)
        {
            TrySwap(ent);
        }

        var bodies = EntityQueryEnumerator<FracturedFormBodyComponent>();
        while (bodies.MoveNext(out var uid, out var _))
        {
            if (!HasComp<SleepingComponent>(uid) && !HasComp<FracturedFormPowerComponent>(uid))
            {
                _sleeping.TrySleeping(uid);
            }
        }
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

        bool hasClothes = true;  // _random.Prob(0.4f);

        EntityUid? newBody;
        if (_random.Prob(0.6f) || !_cloning.TryCloning(original, _transform.GetMapCoordinates(original), hasClothes ? original.Comp.CopyClothed : original.Comp.CopyNaked, out newBody)) // Slightly lower chance to copy the original body
        {
            // Either the dice rolled poorly, or the cloning failed. Either way, make a new body instead. (Or try to)
            var validSpecies = new List<ProtoId<SpeciesPrototype>>();
            var speciesPrototypes = _prototype.EnumeratePrototypes<SpeciesPrototype>();
            foreach (var proto in speciesPrototypes)
            {
                if (proto.RoundStart)
                    validSpecies.Add(proto.ID);
            }
            var species = _random.Pick(validSpecies);
            var character = HumanoidCharacterProfile.RandomWithSpecies(species);
            newBody = _stationSpawning.SpawnPlayerMob(xform.Coordinates, hasClothes ? original.Comp.VisitorJob : "", character, _station.GetCurrentStation(original.Owner));
            if (newBody == null || Deleted(newBody.Value))
            {
                Log.Error($"Failed to create a new body for {ToPrettyString(original)}. This is a bug.");
                return EntityUid.Invalid;
            }
        }

        if (newBody != null && !Deleted(newBody.Value))
        {
            AddComp<FracturedFormBodyComponent>(newBody.Value);
            original.Comp.Bodies.Add(newBody.Value);
            return newBody!.Value;
        }

        return default!;
    }

    private bool TrySwap(Entity<FracturedFormPowerComponent> entity)
    {
        // Pick a random body, or the other one, if we have more than one.
        // Only picks bodies which don't have actives minds (In case of mindswap or something stupid)
        var bodies = entity.Comp.Bodies
            .Where(c =>
                c != entity.Owner                                 // Not the current body
             && TryComp<MindContainerComponent>(c, out var cmind) // Has a mindcontainer still
             && !cmind.HasMind                                    // Is not current possessed somehow
             && !HasComp<ForcedSleepingComponent>(c)              // Not forcefully unconsious either
             && !_mobState.IsIncapacitated(c))                    // Isn't bleeding out somewhere (Or dead)
            .ToList();
        if (!bodies.Any())
            return false;
        var targetBody = _random.Pick(bodies);
        _audio.PlayPredicted(entity.Comp.SwapSound, entity, entity);
        if (TryComp<MindContainerComponent>(entity, out var mindContainer))
            _mind.TransferTo(mindContainer.Mind!.Value, targetBody);

        // If the body is sleeping, try to wake up.
        _sleeping.TryWaking(targetBody, false, entity);

        var duplicate = AddComp<FracturedFormPowerComponent>(targetBody);
        duplicate.Bodies = entity.Comp.Bodies;
        RemCompDeferred(entity, entity.Comp);
        return true;
    }

    private void OnPowerUsed(Entity<FracturedFormPowerComponent> entity, ref FracturedFormPowerActionEvent args)
    {
        if (!TrySwap(entity))
        {
            _popups.PopupEntity(Loc.GetString("fractured-form-nobodies"), entity, entity, PopupType.Large);
            return;
        }

        _psionics.LogPowerUsed(entity, "fractured form swap", 1, 3);
        args.Handled = true;
    }
}
