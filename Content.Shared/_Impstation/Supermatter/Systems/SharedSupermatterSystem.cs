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
    /// This is used for the system's API.
    /// </summary>
    protected EntityQuery<SupermatterComponent> SupermatterQuery;

    protected override string SawmillName => "supermatter";

    [PublicAPI]
    public SupermatterStatusType CalculateSupermatterStatus(Entity<SupermatterComponent?> ent)
    {
        if (!SupermatterQuery.Resolve( ent, ref ent.Comp))
            return SupermatterStatusType.Error;
        
        if (ent.Comp.GasStorage is null)
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
            
            if (sm.DelaminationTime.HasValue && sm.DelaminationTime <= Timing.CurTime)
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
        if (req.MinPower.HasValue && ent.Comp.Power < req.MinPower.Value)
            return false;

        if (req.MaxPower.HasValue && ent.Comp.Power > req.MaxPower.Value)
            return false;

        if (req.MinGlimmer.HasValue && Glimmer.Glimmer < req.MinGlimmer.Value)
            return false;

        if (req.MaxGlimmer.HasValue && Glimmer.Glimmer > req.MaxGlimmer.Value)
            return false;

        var absorbedMoles = ent.Comp.GasStorage?.TotalMoles ?? 0;

        if (req.MinMoles.HasValue && absorbedMoles < req.MinMoles.Value)
            return false;

        if (req.MaxMoles.HasValue && absorbedMoles > req.MaxMoles.Value)
            return false;

        if (req.GasMoles != null && req.GasMoles.Count > 0)
        {
            if (ent.Comp.GasStorage == null)
                return false;

            foreach (var (gas, minMoles) in req.GasMoles)
            {
                if (ent.Comp.GasStorage.GetMoles(gas) < minMoles)
                    return false;
            }
        }
        
        return true;
    }

    protected TimeSpan GetAnnouncementDelay(Entity<SupermatterComponent?> entity)
    {
        if (!SupermatterQuery.Resolve(entity, ref entity.Comp))
            return TimeSpan.Zero;

        if (entity.Comp.IsDelaminating && entity.Comp.DelaminationTime.HasValue)
        {
            return (entity.Comp.DelaminationTime.Value.TotalSeconds - Timing.CurTime.TotalSeconds) switch
            {
                > 30 => TimeSpan.FromSeconds(10),
                > 5 => TimeSpan.FromSeconds(5),
                <= 5 => TimeSpan.FromSeconds(1),
                _ => TimeSpan.FromSeconds(10)
            };
        }

        return entity.Comp.AnnounceInterval.TotalSeconds >= 1.0 ? entity.Comp.AnnounceInterval : TimeSpan.FromSeconds(1);
    }

    protected void SetNextAnnouncementTime(Entity<SupermatterComponent?> entity, TimeSpan delay)
    {
        if (!SupermatterQuery.Resolve(entity, ref entity.Comp))
            return;
        
        entity.Comp.AnnounceNext = Timing.CurTime + delay ;
        DirtyField(entity, entity.Comp, nameof(SupermatterComponent.AnnounceNext));
    }

    protected void SetNextAnnouncementTime(Entity<SupermatterComponent?> entity)
    {
        if (!SupermatterQuery.Resolve(entity, ref entity.Comp))
            return;
        
        entity.Comp.AnnounceNext = Timing.CurTime + GetAnnouncementDelay(entity);
        DirtyField(entity, entity.Comp, nameof(SupermatterComponent.AnnounceNext));
    }

    protected void UpdateLinkedPorts(Entity<SupermatterComponent> ent)
    {
        if (!LinkQuery.HasComp(ent))
            return;
        
        var port = ent.Comp.Status switch
        {
            SupermatterStatusType.Normal => ent.Comp.PortNormal,
            SupermatterStatusType.Caution => ent.Comp.PortCaution,
            SupermatterStatusType.Warning => ent.Comp.PortWarning,
            SupermatterStatusType.Danger => ent.Comp.PortDanger,
            SupermatterStatusType.Emergency => ent.Comp.PortEmergency,
            SupermatterStatusType.Delaminating => ent.Comp.PortDelaminating,
            _ => ent.Comp.PortInactive
        };

        Link.InvokePort(ent, port);
    }

    private void OnExamine(EntityUid uid, SupermatterComponent sm, ref ExaminedEvent args)
    {
        // For ghosts: alive players can use the console
        if (GhostQuery.HasComp(args.Examiner) && args.IsInDetailsRange)
            args.PushMarkup(Loc.GetString("supermatter-examine-integrity", ("integrity", GetIntegrity((uid, sm)).ToString("0.00"))));
    }

    private void OnMapInit(EntityUid uid, SupermatterComponent sm, MapInitEvent args)
    {
        // Set the sound
        Ambient.SetAmbience(uid, true);

        // Send the inactive port for any linked devices
        if (LinkQuery.HasComp(uid))
            Link.InvokePort(uid, sm.PortInactive);
    }

    /// <summary>
    /// Prevents the observing mob from being granted components by a <see cref="GrantComponentsOnObservationComponent"/> if the source is a supermatter.
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="args"></param>
    private void OnObserverGrantedComponents(Entity<SupermatterHallucinationImmuneComponent> ent, ref ObserverGrantedComponents args)
    {
        if (args.Cancelled || !SupermatterQuery.HasComp(args.Source))
            // We are already canceled, or the source is not a supermatter.
            return;
        
        args.Cancel();
    }
}
