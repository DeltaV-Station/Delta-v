using Content.Shared._DV.ChronicPain.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using JetBrains.Annotations;

namespace Content.Shared._DV.ChronicPain.EntitySystems;

public abstract partial class SharedChronicPainSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] protected readonly IPrototypeManager ProtoManager = default!;
    [Dependency] protected readonly IRobustRandom RobustRandom = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] protected readonly StatusEffectsSystem StatusEffects = default!;
    public static readonly EntProtoId ChronicPainStatusEffect = "StatusEffectChronicPainSuppressed";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChronicPainComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ChronicPainComponent, ComponentInit>(OnChronicPainInit);
        SubscribeLocalEvent<ChronicPainComponent, ComponentShutdown>(OnChronicPainShutdown);
        SubscribeLocalEvent<ChronicPainComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ChronicPainComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
    }

    [PublicAPI]
    public bool IsChronicPainSuppressed(Entity<ChronicPainComponent?> entity)
    {
        // If they don't have the Chronic Pain trait, technically their pain was suppressed
        if (!Resolve(entity, ref entity.Comp, false))
            return true;

        return StatusEffects.HasEffectComp<ChronicPainSuppressedStatusEffectComponent>(entity.Owner);
    }

    [PublicAPI]
    public bool TrySuppressChronicPain(Entity<ChronicPainComponent?> entity, TimeSpan? duration)
    {
        // If they don't have the Chronic Pain trait, technically their pain was suppressed
        if (!Resolve(entity, ref entity.Comp, false))
            return true;

        if (!duration.HasValue)
            duration = entity.Comp.DefaultSuppressionTime;

        return StatusEffects.TryAddStatusEffectDuration(entity.Owner, ChronicPainStatusEffect, duration.Value);
    }

    protected void OnMapInit(Entity<ChronicPainComponent> entity, ref MapInitEvent args)
    {
        entity.Comp.NextUpdateTime = _timing.CurTime;
        entity.Comp.NextPopupTime = _timing.CurTime;
    }

    protected virtual void OnChronicPainInit(Entity<ChronicPainComponent> entity, ref ComponentInit args)
    {
        // Give the player a bit of time before they have to take a pill
        if (TrySuppressChronicPain((entity.Owner, entity.Comp), entity.Comp.DefaultSuppressionTimeOnInit))
            entity.Comp.NextUpdateTime = _timing.CurTime + entity.Comp.DefaultSuppressionTimeOnInit;
    }

    protected virtual void OnChronicPainShutdown(Entity<ChronicPainComponent> entity, ref ComponentShutdown args)
    {
        StatusEffects.TryRemoveStatusEffect(entity, ChronicPainStatusEffect);
    }

    protected virtual void OnPlayerAttached(Entity<ChronicPainComponent> entity, ref LocalPlayerAttachedEvent args)
    {
        // Used by the client to add the overlay if not suppressed
    }

    protected virtual void OnPlayerDetached(Entity<ChronicPainComponent> entity, ref LocalPlayerDetachedEvent args)
    {
        // Used by the client to get rid of the overlay
    }

    protected void ShowPainPopup(Entity<ChronicPainComponent> entity)
    {
        // Don't notify
        if (IsChronicPainSuppressed((entity.Owner, entity.Comp)))
            return;

        if (!ProtoManager.TryIndex(entity.Comp.DatasetPrototype, out var dataset))
            return;

        var effects = dataset.Values;
        if (effects.Count == 0)
            return;

        var effect = RobustRandom.Pick(effects);
        Popup.PopupPredicted(Loc.GetString(effect), entity, entity);

        // Set next popup time
        var delay = RobustRandom.Next(entity.Comp.MinimumPopupDelay, entity.Comp.MaximumPopupDelay);
        entity.Comp.NextPopupTime = _timing.CurTime + delay;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<ChronicPainComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            if (curTime < component.NextUpdateTime)
                continue;

            if (curTime >= component.NextPopupTime)
                ShowPainPopup((uid, component));

            component.NextUpdateTime = curTime + TimeSpan.FromSeconds(5);
        }
    }
}
