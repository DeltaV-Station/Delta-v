using Content.Shared._DV.Mind;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Mind.Components;
using Content.Shared.Speech;
using Content.Shared.Stealth.Components;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

public abstract class SharedTelegnosisPowerSystem : BasePsionicPowerSystem<TelegnosisPowerComponent, TelegnosisPowerActionEvent>
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TelegnosticProjectionComponent, MindRemovedMessage>(OnMindRemoved);
        SubscribeLocalEvent<TelegnosticProjectionComponent, InteractionAttemptEvent>(OnInteractionAttempt);
        SubscribeLocalEvent<TelegnosticProjectionComponent, MindSwapLinkSeveredEvent>(OnLinkSevered);
        SubscribeLocalEvent<TelegnosisPowerComponent, ShowSSDIndicatorEvent>(OnShowSSDIndicator);
        SubscribeLocalEvent<TelegnosisPowerComponent, ExaminedEvent>(OnExamine);
    }

    private void OnMindRemoved(Entity<TelegnosticProjectionComponent> projection, ref MindRemovedMessage args)
    {
        QueueDel(projection);
    }

    private void OnInteractionAttempt(Entity<TelegnosticProjectionComponent> projection, ref InteractionAttemptEvent args)
    {
        // no astrally stealing someone's shoes
        args.Cancelled = true;
    }

    private void OnLinkSevered(Entity<TelegnosticProjectionComponent> victim, ref MindSwapLinkSeveredEvent args)
    {
        RemComp<PsionicallyInvisibleComponent>(victim);
        RemComp<StealthComponent>(victim);
        EnsureComp<SpeechComponent>(victim);
        EnsureComp<DispellableComponent>(victim);
        _metaData.SetEntityName(victim, Loc.GetString("telegnostic-trapped-entity-name"));
        _metaData.SetEntityDescription(victim, Loc.GetString("telegnostic-trapped-entity-desc"));
    }

    private void OnShowSSDIndicator(Entity<TelegnosisPowerComponent> psionic, ref ShowSSDIndicatorEvent args)
    {
        if (!TryComp<MindSwappedReturnPowerComponent>(psionic, out var mindSwapped) || !HasComp<TelegnosticProjectionComponent>(mindSwapped.OriginalEntity))
            return;
        // Only hide if currently projecting
        args.Hidden = true;
    }

    private void OnExamine(Entity<TelegnosisPowerComponent> entity, ref ExaminedEvent args)
    {
        if (GetCasterProjection(entity) == default)
            return;

        args.PushMarkup($"[color=yellow]{Loc.GetString("telegnosis-power-ssd", ("ent", entity))}[/color]");
    }

    public EntityUid GetCasterProjection(Entity<TelegnosisPowerComponent> entity)
    {
        if (!TryComp<MindSwappedReturnPowerComponent>(entity, out var mindSwapped) ||
            !HasComp<TelegnosticProjectionComponent>(mindSwapped.OriginalEntity))
        {
            return default;
        }
        return mindSwapped.OriginalEntity;
    }
}
