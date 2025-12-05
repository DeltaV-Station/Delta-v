using Content.Server.Abilities.Psionics;
using Content.Server.Chat.Systems;
using Content.Server.Cloning;
using Content.Server.DoAfter;
using Content.Server.Mind;
using Content.Server.Psionics;
using Content.Server.Station.Systems;
using Content.Shared._DV.Abilities.Psionics;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Bed.Sleep;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Preferences;
using Content.Shared.Psionics.Events;
using Content.Shared.SSDIndicator;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.Abilities.Psionics;

public sealed class FracturedFormPowerSystem : SharedFracturedFormPowerSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly CloningSystem _cloning = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
    [Dependency] private readonly SleepingSystem _sleeping = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FracturedFormPowerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<FracturedFormPowerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FracturedFormPowerComponent, FracturedFormPowerActionEvent>(OnPowerUsed);
        SubscribeLocalEvent<FracturedFormPowerComponent, DispelledEvent>(OnDispelled);
        SubscribeLocalEvent<FracturedFormPowerComponent, FracturedFormDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<FracturedFormBodyComponent, ExaminedEvent>(OnExamine);
    }

    private void OnInit(Entity<FracturedFormPowerComponent> entity, ref ComponentInit args)
    {
        var component = entity.Comp;
        _actions.AddAction(entity, ref component.FracturedFormActionEntity, component.FracturedFormActionId);
        _actions.StartUseDelay(component.FracturedFormActionEntity);
        if (TryComp<PsionicComponent>(entity, out var psionic) && psionic.PsionicAbility == null)
        {
            psionic.PsionicAbility = component.FracturedFormActionEntity;
            psionic.ActivePowers.Add(component);
        }

        // Next random swap is between 5 to 20 minutes.
        component.NextSwap = _timing.CurTime + TimeSpan.FromSeconds(_random.Next(300, 1200));

        if (HasComp<FracturedFormBodyComponent>(entity)) return; // Don't generate a new body if we're already part of a network.
        var bodyComp = AddComp<FracturedFormBodyComponent>(entity);
        bodyComp.ControllingForm = entity.Owner;
        component.Bodies.Add(entity);
        GenerateForm(entity);
    }

    private void OnShutdown(Entity<FracturedFormPowerComponent> entity, ref ComponentShutdown args)
    {
        _actions.RemoveAction(entity.Owner, entity.Comp.FracturedFormActionEntity);

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
                _chat.TryEmoteWithChat(uid, "Yawn", ChatTransmitRange.Normal);
            }
            if (HasComp<SleepingComponent>(uid) || _mobState.IsIncapacitated(uid) || t > comp.NextSwap)
            {
                toSwap.Add(new(uid, comp));
            }
        }
        foreach (var ent in toSwap)
        {
            TrySwap(ent);
        }

        var bodies = EntityQueryEnumerator<FracturedFormBodyComponent>();
        while (bodies.MoveNext(out var uid, out var comp))
        {
            if (!HasComp<SleepingComponent>(uid) && !_mind.GetMind(uid).HasValue)
            {
                _sleeping.TrySleeping(uid);
            }

            if (TryComp<SSDIndicatorComponent>(uid, out var ssd) && ssd.IsSSD)
            {
                // Ensure the body isn't forcesleep'd by the SSD system.
                ssd.IsSSD = false;
            }

            // Cleanup the component from any out-of-network bodies. (See Mindbreaking)
            if (!comp.ControllingForm.IsValid() || Deleted(comp.ControllingForm) || !HasComp<FracturedFormPowerComponent>(comp.ControllingForm))
                RemCompDeferred<FracturedFormBodyComponent>(uid);
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

        bool hasClothes = _random.Prob(0.4f);

        EntityUid? newBody;
        if (_random.Prob(0.6f) || !_cloning.TryCloning(original, _transform.GetMapCoordinates(original), hasClothes ? original.Comp.CopyClothed : original.Comp.CopyNaked, out newBody)) // Slightly lower chance to copy the original body
        {
            // Either the dice rolled poorly, or the cloning failed. Either way, make a new body instead. (Or try to)
            var validSpecies = new List<ProtoId<SpeciesPrototype>>();
            var speciesPrototypes = _prototype.EnumeratePrototypes<SpeciesPrototype>();
            foreach (var proto in speciesPrototypes)
            {
                var speciesEntityPrototype = _prototype.Index<EntityPrototype>(proto.Prototype);

                if (proto.RoundStart && speciesEntityPrototype.TryGetComponent<PotentialPsionicComponent>(out var canBePsionic, Factory))
                {
                    var chance = canBePsionic.Chance;

                    if (speciesEntityPrototype.TryGetComponent<PsionicBonusChanceComponent>(out var bonusChance, Factory))
                        chance = (chance * bonusChance.Multiplier) + bonusChance.FlatBonus;

                    if (chance > 0)
                        validSpecies.Add(proto.ID);
                }
            }
            var species = _random.Pick(validSpecies);
            var character = HumanoidCharacterProfile.RandomWithSpecies(species);
            newBody = _stationSpawning.SpawnPlayerMob(xform.Coordinates, hasClothes ? original.Comp.VisitorJob : original.Comp.NakedJob, character, _station.GetOwningStation(original.Owner));
            if (newBody is not { } bodyV || Deleted(bodyV))
            {
                Log.Error($"Failed to create a new body for {ToPrettyString(original)}. This is a bug.");
                return EntityUid.Invalid;
            }
        }

        if (newBody is { } body && !Deleted(body))
        {
            var bodyComp = AddComp<FracturedFormBodyComponent>(body);
            original.Comp.Bodies.Add(body);
            bodyComp.ControllingForm = original.Owner;
            return body;
        }

        return default!;
    }

    private bool IsValidBody(Entity<FracturedFormPowerComponent> entity, EntityUid body)
    {
        if (body == entity.Owner)
            return false;
        if (!entity.Comp.Bodies.Contains(body))
            return false;
        if (!TryComp<MindContainerComponent>(body, out var cmind))
            return false;
        if (cmind.HasMind)
            return false;
        if (HasComp<ForcedSleepingStatusEffectComponent>(body))
            return false;
        if (_mobState.IsIncapacitated(body))
            return false;
        return true;
    }

    private List<EntityUid> ValidBodies(Entity<FracturedFormPowerComponent> entity)
    {
        var bodies = new List<EntityUid>();
        foreach (var body in entity.Comp.Bodies)
        {
            if (IsValidBody(entity, body))
                bodies.Add(body);
        }
        return bodies;
    }

    public bool CanSwap(Entity<FracturedFormPowerComponent> entity)
    {
        foreach (var body in entity.Comp.Bodies)
        {
            if (IsValidBody(entity, body))
                return true;
        }
        return false;
    }

    private bool TrySwap(Entity<FracturedFormPowerComponent> entity)
    {
        // Pick a random body, or the other one, if we have more than one.
        // Only picks bodies which don't have actives minds (In case of mindswap or something stupid)
        if (!CanSwap(entity))
            return false;
        var targetBody = _random.Pick(ValidBodies(entity));
        _audio.PlayPredicted(entity.Comp.SwapSound, entity, entity);
        if (TryComp<MindContainerComponent>(entity, out var mindContainer))
            _mind.TransferTo(mindContainer.Mind!.Value, targetBody);

        // If the body is sleeping, try to wake up.
        _sleeping.TryWaking(targetBody, false, entity);

        var duplicate = AddComp<FracturedFormPowerComponent>(targetBody);
        duplicate.Bodies = entity.Comp.Bodies;
        // Pass the controlling form to the new component's bodies.
        foreach (var body in duplicate.Bodies)
        {
            if (TryComp<FracturedFormBodyComponent>(body, out var bodyComp))
            {
                bodyComp.ControllingForm = targetBody;
            }
        }
        RemCompDeferred(entity, entity.Comp);
        return true;
    }

    private void OnPowerUsed(Entity<FracturedFormPowerComponent> entity, ref FracturedFormPowerActionEvent args)
    {
        if (!CanSwap(entity))
        {
            _popups.PopupEntity(Loc.GetString("fractured-form-nobodies"), entity, entity, PopupType.Large);
            return;
        }

        entity.Comp.SleepWarned = true;
        _chat.TryEmoteWithChat(entity.Owner, "Yawn", ChatTransmitRange.Normal);
        _popups.PopupEntity(Loc.GetString("fractured-form-sleepy"), entity, entity, PopupType.LargeCaution);
        var ev = new FracturedFormDoAfterEvent();
        var doAfterArgs = new DoAfterArgs(EntityManager, entity, entity.Comp.ManualSwapTime, ev, entity);
        _doAfter.TryStartDoAfter(doAfterArgs, out var doAfterId);
        entity.Comp.DoAfter = doAfterId;
        _psionics.LogPowerUsed(entity, "fractured form swap", 1, 3);

        args.Handled = true;
    }

    private void OnDispelled(Entity<FracturedFormPowerComponent> entity, ref DispelledEvent args)
    {
        if (entity.Comp.DoAfter == null)
            return;

        _doAfter.Cancel(entity.Comp.DoAfter);
        entity.Comp.DoAfter = null;

        args.Handled = true;
    }

    private void OnDoAfter(Entity<FracturedFormPowerComponent> entity, ref FracturedFormDoAfterEvent args)
    {
        entity.Comp.DoAfter = null;

        if (args.Cancelled || args.Handled)
            return;

        _sleeping.TrySleeping(entity.Owner);
    }

    private void OnExamine(Entity<FracturedFormBodyComponent> entity, ref ExaminedEvent args)
    {
        if (HasComp<FracturedFormPowerComponent>(entity))
            return;
        if (TryComp<FracturedFormPowerComponent>(args.Examiner, out var fracturedHost) && fracturedHost.Bodies.Contains(entity.Owner))
            args.PushMarkup($"[color=yellow]{Loc.GetString("fractured-form-examine-self", ("ent", entity))}[/color]");
        else
            args.PushMarkup($"[color=yellow]{Loc.GetString("fractured-form-ssd", ("ent", entity))}[/color]");
    }
}
