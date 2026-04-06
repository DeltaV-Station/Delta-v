using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Content.Shared.Stealth.Components;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

/// <summary>
/// Sorry if this is shitcode, but the return power actually should behave like a normal power - So it gets its own system.
/// That way, we have automatical power inits, dispelled and mindbreaking, as well as checks for if someone can use a power.
/// This is NOT its own power. It can ONLY exist if someone used a mindswap or a minor mass mind swap happened.
/// </summary>
public sealed class MindSwappedReturnPowerSystem : BasePsionicPowerSystem<MindSwappedReturnPowerComponent, MindSwappedReturnPowerActionEvent>
{
    [Dependency] private readonly SharedMindSwapPowerSystem _mindSwap = default!;
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;

    private EntityQuery<MindSwappedReturnPowerComponent> _mindSwappedQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindSwappedReturnPowerComponent, ComponentShutdown>(OnShutDown);

        _mindSwappedQuery = GetEntityQuery<MindSwappedReturnPowerComponent>();
    }

    private void OnShutDown(Entity<MindSwappedReturnPowerComponent> psionic, ref ComponentShutdown args)
    {
        // If the person is gibbed or otherwise deleted, it'll remove the links.
        if (Timing.ApplyingState
            || !TerminatingOrDeleted(psionic)
            || !_mindSwappedQuery.TryComp(psionic.Comp.OriginalEntity, out var targetComp))
            return;

        RemoveLink((psionic.Comp.OriginalEntity, targetComp));
    }

    protected override void OnPowerUsed(Entity<MindSwappedReturnPowerComponent> psionic, ref MindSwappedReturnPowerActionEvent args)
    {
        _mindSwap.SwapMinds(psionic, psionic.Comp.OriginalEntity);
        AfterPowerUsed(psionic, args.Performer);
    }

    protected override void OnDispelled(Entity<MindSwappedReturnPowerComponent> psionic, ref DispelledEvent args)
    {
        _mindSwap.SwapMinds(psionic, psionic.Comp.OriginalEntity, false);
    }

    public void RemoveLink(Entity<MindSwappedReturnPowerComponent?> victim, bool showPopup = true)
    {
        // Sometimes people lose their link without having the component - MassMindSwap for example is a situation like that.
        if (showPopup)
            Popup.PopupEntity(Loc.GetString("psionic-power-mindswap-original-lost"), victim, victim, PopupType.MediumCaution);

        if (!Resolve(victim, ref victim.Comp, false))
            return;
        // Remove the first action and link.
        Action.RemoveAction(victim.Comp.ActionEntity);
        RemCompDeferred(victim, victim.Comp);

        if (!HasComp<TelegnosticProjectionComponent>(victim))
            return;

        RemComp<PsionicallyInvisibleComponent>(victim);
        RemComp<StealthComponent>(victim);
        EnsureComp<SpeechComponent>(victim);
        EnsureComp<DispellableComponent>(victim);
        _metaDataSystem.SetEntityName(victim, Loc.GetString("telegnostic-trapped-entity-name"));
        _metaDataSystem.SetEntityDescription(victim, Loc.GetString("telegnostic-trapped-entity-desc"));
    }
}
