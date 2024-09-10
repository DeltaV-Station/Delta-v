using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared.Abilities.Psionics;

public sealed class SharedPsionicAbilitiesSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PsionicsDisabledComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<PsionicsDisabledComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PsionicComponent, PsionicPowerUsedEvent>(OnPowerUsed);

        SubscribeLocalEvent<PsionicComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnPowerUsed(EntityUid uid, PsionicComponent component, PsionicPowerUsedEvent args)
    {
        var ev = new PsionicPowerDetectedEvent(uid, args.Power);
        var coords = Transform(uid).Coordinates;
        foreach (var ent in _lookup.GetEntitiesInRange<MetapsionicPowerComponent>(coords, 10f))
        {
            if (ent.Owner != uid && !(TryComp<PsionicInsulationComponent>(ent, out var insul) && !insul.Passthrough))
            {
                RaiseLocalEvent(ent, ref ev);
                _popups.PopupEntity(Loc.GetString("metapsionic-pulse-power", ("power", args.Power)), ent, ent, PopupType.LargeCaution);
                args.Handled = true;
            }
        }
    }

    private void OnInit(EntityUid uid, PsionicsDisabledComponent component, ComponentInit args)
    {
        SetPsionicsThroughEligibility(uid);
    }

    private void OnShutdown(EntityUid uid, PsionicsDisabledComponent component, ComponentShutdown args)
    {
        SetPsionicsThroughEligibility(uid);
    }

    private void OnMobStateChanged(EntityUid uid, PsionicComponent component, MobStateChangedEvent args)
    {
        SetPsionicsThroughEligibility(uid);
    }

    /// <summary>
    /// Checks whether the entity is eligible to use its psionic ability. This should be run after anything that could effect psionic eligibility.
    /// </summary>
    public void SetPsionicsThroughEligibility(EntityUid uid)
    {
        PsionicComponent? component = null;
        if (!Resolve(uid, ref component, false))
            return;

        if (component.PsionicAbility == null)
            return;

        _actions.TryGetActionData( component.PsionicAbility, out var actionData );

        if (actionData == null)
            return;

        _actions.SetEnabled(actionData.Owner, IsEligibleForPsionics(uid));
    }

    private bool IsEligibleForPsionics(EntityUid uid)
    {
        return !HasComp<PsionicInsulationComponent>(uid)
            && (!TryComp<MobStateComponent>(uid, out var mobstate) || mobstate.CurrentState == MobState.Alive);
    }

    public void LogPowerUsed(EntityUid uid, string power, int minGlimmer = 8, int maxGlimmer = 12)
    {
        _adminLogger.Add(Database.LogType.Psionics, Database.LogImpact.Medium, $"{ToPrettyString(uid):player} used {power}");
        var ev = new PsionicPowerUsedEvent(uid, power);
        RaiseLocalEvent(uid, ev, false);

        _glimmerSystem.Glimmer += _robustRandom.Next(minGlimmer, maxGlimmer);
    }
}

/// <summary>
/// Event raised on a metapsionic entity when someone used a psionic power nearby.
/// </summary>
[ByRefEvent]
public record struct PsionicPowerDetectedEvent(EntityUid Psionic, string Power);

public sealed class PsionicPowerUsedEvent : HandledEntityEventArgs
{
    public EntityUid User { get; }
    public string Power = string.Empty;

    public PsionicPowerUsedEvent(EntityUid user, string power)
    {
        User = user;
        Power = power;
    }
}

[Serializable]
[NetSerializable]
public sealed class PsionicsChangedEvent : EntityEventArgs
{
    public readonly NetEntity Euid;
    public PsionicsChangedEvent(NetEntity euid)
    {
        Euid = euid;
    }
}
