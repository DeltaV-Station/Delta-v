using System.Linq;
using Content.Shared._DV.Vision.Components;
using Content.Shared._Impstation.CCVar;
using Content.Shared._Impstation.Supermatter.Components;
using Content.Shared._Impstation.Supermatter.Prototypes;
using Content.Shared.Atmos;
using Content.Shared.DeviceLinking;
using Content.Shared.EntityEffects;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Psionics.Glimmer;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
namespace Content.Shared._Impstation.Supermatter.Systems;

/// <summary>
/// This handles the processing for Supermatter Crystals
/// </summary>
public abstract partial class SharedSupermatterSystem : EntitySystem
{
    [Dependency] protected readonly IConfigurationManager Config = default!;
    [Dependency] protected readonly SharedContainerSystem Container = default!;
    [Dependency] protected readonly SharedEntityEffectsSystem Effects = default!;
    [Dependency] protected readonly EntityLookupSystem EntityLookup = default!;
    [Dependency] protected readonly GlimmerSystem Glimmer = default!;
    [Dependency] protected readonly SharedDeviceLinkSystem Link = default!;
    [Dependency] protected readonly SharedMapSystem Map = default!;
    [Dependency] protected readonly IRobustRandom Random = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;

    /// <summary>
    /// This is used to let ghosts see the integrity of the supermatter and to make sure we don't consume any items held by ghosts.
    /// </summary>
    protected EntityQuery<GhostComponent> GhostQuery;

    /// <summary>
    /// This is used for device linking and signals.
    /// </summary>
    protected EntityQuery<DeviceLinkSourceComponent> LinkQuery;

    /// <summary>
    /// Psychological soothing is used to increase the maximum temperature at which the supermatter can operate without overheating, as well as decreasing waste production.
    /// </summary>
    protected EntityQuery<PsychologicalSoothingReceiverComponent> PsyReceiversQuery;

    /// <summary>
    /// This is used for this system's API.
    /// </summary>
    protected EntityQuery<SupermatterComponent> SupermatterQuery;

    protected override string SawmillName => "supermatter";

    [PublicAPI]
    public SupermatterStatusType CalculateSupermatterStatus(Entity<SupermatterComponent?> ent)
    {
        if (!SupermatterQuery.Resolve( ent, ref ent.Comp) || ent.Comp.GasStorage is null)
            return SupermatterStatusType.Error;

        if (ent.Comp.IsDelaminating || ent.Comp.Damage >= ent.Comp.DamageDelaminationThreshold)
            return SupermatterStatusType.Delaminating;

        if (ent.Comp.Damage >= ent.Comp.DamageEmergencyThreshold)
            return SupermatterStatusType.Emergency;

        if (ent.Comp.Damage >= ent.Comp.DamageDangerThreshold)
            return SupermatterStatusType.Danger;

        if (ent.Comp.Damage >= ent.Comp.DamageWarningThreshold)
            return SupermatterStatusType.Warning;

        if (ent.Comp.GasStorage.Temperature > Atmospherics.T0C + Config.GetCVar(ImpCCVars.SupermatterHeatPenaltyThreshold) * 0.8)
            return SupermatterStatusType.Caution;

        if (ent.Comp.Power > 5)
            return SupermatterStatusType.Normal;

        return SupermatterStatusType.Inactive;
    }

    [PublicAPI]
    public float GetGasEfficiency(Entity<SupermatterComponent?> ent)
    {
        if (!SupermatterQuery.Resolve(ent, ref ent.Comp))
            return 0f;

        return ent.Comp.GasEfficiency / (ent.Comp.Power > 0 ? 1 : Config.GetCVar(ImpCCVars.SupermatterGasEfficiencyGraceModifier));
    }

    /// <summary>
    /// Returns the integrity rounded to hundreds, e.g. 100.00%
    /// </summary>
    [PublicAPI]
    public float GetIntegrity(Entity<SupermatterComponent?> ent)
    {
        if (!SupermatterQuery.Resolve(ent, ref ent.Comp))
            return 0f;

        var integrity = ent.Comp.Damage / ent.Comp.DamageDelaminationThreshold;
        integrity = (float)Math.Round(100 - integrity * 100, 2);
        integrity = integrity < 0 ? 0 : integrity;
        return integrity;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        
        InitializeCosmetic();
        
        GhostQuery = GetEntityQuery<GhostComponent>();
        LinkQuery = GetEntityQuery<DeviceLinkSourceComponent>();
        PsyReceiversQuery = GetEntityQuery<PsychologicalSoothingReceiverComponent>();
        SupermatterQuery = GetEntityQuery<SupermatterComponent>();
        
        SubscribeLocalEvent<SupermatterComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SupermatterComponent, ExaminedEvent>(OnExamine);
        
        SubscribeLocalEvent<SupermatterComponent, SupermatterDelaminationEvent>(OnSupermatterDelamination);
        
        SubscribeLocalEvent<SupermatterHallucinationImmuneComponent, ObserverGrantedComponents>(OnObserverGrantedComponents);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<SupermatterComponent>();
        
        while (query.MoveNext(out var uid, out var sm))
        {
            if(Paused(uid)) continue;
            
            if (sm.DelaminationTime is { } delaminationTime && delaminationTime <= Timing.CurTime)
            {
                var ev = new SupermatterDelaminationEvent();
                RaiseLocalEvent(uid, ref ev, true);
                continue;
            }

            UpdateSupermatter((uid, sm), frameTime);
        }
        
        base.Update(frameTime);
    }
    
    protected abstract void OnSupermatterDelamination(EntityUid uid, SupermatterComponent sm, SupermatterDelaminationEvent args);

    protected abstract void UpdateSupermatter(Entity<SupermatterComponent> ent, float frameTime);

    protected bool CheckDelaminationRequirements(Entity<SupermatterComponent> ent, SupermatterDelaminationRequirements req) 
    {
        if (req.MinPower is {} minPower && ent.Comp.Power < minPower)
            return false;
        
        if (req.MaxPower is {} maxPower && ent.Comp.Power > maxPower)
            return false;

        if (req.MinGlimmer is {} minGlimmer && Glimmer.Glimmer < minGlimmer)
            return false;

        if (req.MaxGlimmer is {} maxGlimmer && Glimmer.Glimmer > maxGlimmer)
            return false;

        var absorbedMoles = ent.Comp.GasStorage?.TotalMoles ?? 0;

        if (req.MinMoles is {} minMoles && absorbedMoles < minMoles)
            return false;

        if (req.MaxMoles is {} maxMoles && absorbedMoles > maxMoles)
            return false;

        if (req.GasRatio is { Count: > 0 })
        {
            if (ent.Comp.GasStorage is null)
                return false;

            var ratio = GetGasRatio(ent.Comp.GasStorage);

            foreach (var (gas, minRatio) in req.GasRatio)
            {
                if (ratio[gas] < minRatio)
                    return false;
            }
        }
        
        return true;
    }
    
    protected Dictionary<Gas, float> GetGasRatio(GasMixture? mix)
    {
        if (mix is null)
            return new();

        var totalMoles = mix.TotalMoles;
        
        return mix.ToDictionary(
            pair => pair.gas,
            pair => Math.Clamp(pair.moles / totalMoles, 0f, 1f)
        );
    }

    private void OnExamine(EntityUid uid, SupermatterComponent sm, ref ExaminedEvent args)
    {
        // This is for ghosts only, because alive players should just use the console.
        if (GhostQuery.HasComp(args.Examiner) && args.IsInDetailsRange)
            args.PushMarkup(Loc.GetString("supermatter-examine-integrity", ("integrity", GetIntegrity((uid, sm)).ToString("0.00"))));
    }

    private void OnMapInit(EntityUid uid, SupermatterComponent sm, MapInitEvent args)
    {
        Ambient.SetAmbience(uid, true);

        // Invoke the inactive port on map init for any pre-mapped setups.
        if (LinkQuery.HasComp(uid) && sm.SignalPorts.TryGetValue(SupermatterStatusType.Inactive, out var port))
            Link.InvokePort(uid, port);
    }

    /// <summary>
    /// Prevents the observing mob from being granted components by a <see cref="GrantComponentsOnObservationComponent"/> if the source is a supermatter.
    /// </summary>
    private void OnObserverGrantedComponents(Entity<SupermatterHallucinationImmuneComponent> ent, ref ObserverGrantedComponents args)
    {
        // We are already canceled, or the source is not a supermatter. Either case we don't need to intervene.
        if (args.Cancelled || !SupermatterQuery.HasComp(args.Source))
            return;

        args.Cancel();
    }
}
