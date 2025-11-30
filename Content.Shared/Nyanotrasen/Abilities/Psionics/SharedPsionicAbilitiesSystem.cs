using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared.Abilities.Psionics;

public sealed partial class SharedPsionicAbilitiesSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly SharedActionsSystem _actionSystem = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();


        SubscribeLocalEvent<PsionicsDisabledComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<PsionicsDisabledComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<PsionicComponent, MobStateChangedEvent>(OnMobStateChanged);
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
        SetPsionicsThroughEligibility(args.Target);
    }

    /// <summary>
    /// Checks whether the entity is eligible to use its psionic ability. This should be run after anything that could effect psionic eligibility.
    /// </summary>
    public void SetPsionicsThroughEligibility(EntityUid psionic, PsionicComponent? psionicComp = null)
    {
        if (!Resolve(psionic, ref psionicComp, false)
            || psionicComp.PsionicPowersActionEntities.Count == 0)
            return;

        var canUsePsionics = IsEligibleForPsionics(psionic);

        foreach (var power in psionicComp.PsionicPowersActionEntities)
        {
            _actionSystem.SetEnabled(power, canUsePsionics);
        }
    }

    private bool IsEligibleForPsionics(EntityUid psionic)
    {
        if (TryComp<PsionicallyInsulatedComponent>(psionic, out var insulComp))
            return insulComp.AllowsPsionicUsage && _mobStateSystem.IsAlive(psionic);

        return _mobStateSystem.IsAlive(psionic);
    }

    public void LogPowerUsed(EntityUid uid, string power, int minGlimmer = 8, int maxGlimmer = 12)
    {
        _adminLogger.Add(Database.LogType.Psionics, Database.LogImpact.Medium, $"{ToPrettyString(uid):player} used {power}");
        var ev = new PsionicPowerUsedEvent(uid, power);
        RaiseLocalEvent(uid, ev, false);

        _glimmerSystem.Glimmer += _robustRandom.Next(minGlimmer, maxGlimmer);
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
