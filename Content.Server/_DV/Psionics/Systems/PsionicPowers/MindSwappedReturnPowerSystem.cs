using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;

namespace Content.Server._DV.Psionics.Systems.PsionicPowers;

public sealed class MindSwappedReturnPowerSystem : SharedMindSwappedReturnPowerSystem
{
    [Dependency] private readonly PsionicSystem _psionic = default!;

    private EntityQuery<MindSwappedReturnPowerComponent> _mindSwappedQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindSwappedReturnPowerComponent, ComponentShutdown>(OnShutDown);
        SubscribeLocalEvent<MindSwappedReturnPowerComponent, AntiPsionicWeaponHitEvent>(OnAntiPsionicHit);

        _mindSwappedQuery = GetEntityQuery<MindSwappedReturnPowerComponent>();
    }

    private void OnShutDown(Entity<MindSwappedReturnPowerComponent> psionic, ref ComponentShutdown args)
    {
        // If the person is gibbed or otherwise deleted, it'll remove the links.
        if (Timing.ApplyingState
            || !TerminatingOrDeleted(psionic)
            || !_mindSwappedQuery.TryComp(psionic.Comp.OriginalEntity, out var targetComp))
            return;

        _psionic.RemoveLink((psionic.Comp.OriginalEntity, targetComp));
    }

    protected override void OnPowerUsed(Entity<MindSwappedReturnPowerComponent> psionic, ref MindSwappedReturnPowerActionEvent args)
    {
        _psionic.SwapMinds(psionic, psionic.Comp.OriginalEntity);
        AfterPowerUsed(psionic, args.Performer);
    }

    protected override void OnDispelled(Entity<MindSwappedReturnPowerComponent> psionic, ref DispelledEvent args)
    {
        _psionic.SwapMinds(psionic, psionic.Comp.OriginalEntity, false);
    }

    private void OnAntiPsionicHit(Entity<MindSwappedReturnPowerComponent> psionic, ref AntiPsionicWeaponHitEvent args)
    {
        _psionic.SwapMinds(psionic, psionic.Comp.OriginalEntity, false);
    }
}
