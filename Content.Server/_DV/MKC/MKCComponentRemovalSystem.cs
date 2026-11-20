using Content.Server.Body.Components;
using Content.Shared._DV.Body.Components;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Body.Components;
using Content.Shared.Nutrition.Components;

namespace Content.Server._DV.MKC;

// ReSharper disable once InconsistentNaming
public sealed class MKCComponentRemovalSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MKCComponentRemovalComponent, ComponentStartup>(OnStartup);
    }

    /// <summary>
    /// Just removes a whole bunch of components from BaseSpeciesMobOrganic that we don't want
    /// </summary>
    private void OnStartup(EntityUid uid, MKCComponentRemovalComponent comp, ref ComponentStartup args)
    {
        // less of a pain than yaml copy paste
        RemComp<InternalsComponent>(uid);
        RemComp<RespiratorComponent>(uid);
        RemComp<AffectedByCPRComponent>(uid);
        RemComp<CanDoCPRComponent>(uid);

        RemComp<HungerComponent>(uid);
        RemComp<ThirstComponent>(uid);

        RemComp<PerishableComponent>(uid);
        RemComp<ButcherableComponent>(uid);
    }
}
