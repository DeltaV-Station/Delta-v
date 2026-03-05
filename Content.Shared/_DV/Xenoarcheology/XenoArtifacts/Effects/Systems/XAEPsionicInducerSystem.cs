using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Systems;
using Content.Shared._DV.Xenoarcheology.XenoArtifacts.Effects.Components;
using Content.Shared.Xenoarchaeology.Artifact;

namespace Content.Shared._DV.Xenoarcheology.XenoArtifacts.Effects.Systems;

public sealed class XAEPsionicInducerSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPsionicSystem _psionic = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XAEPsionicInducerComponent, XenoArtifactNodeActivatedEvent>(OnActivated);
    }

    private void OnActivated(Entity<XAEPsionicInducerComponent> arti, ref XenoArtifactNodeActivatedEvent args)
    {
        var coords = Transform(arti).Coordinates;
        foreach (var target in _lookup.GetEntitiesInRange<PotentialPsionicComponent>(coords, arti.Comp.Range))
        {
            if (HasComp<PsionicComponent>(target))
                continue;

            var ev = new TargetedByPsionicPowerEvent();
            RaiseLocalEvent(target.Owner, ref ev);

            if (!ev.IsShielded)
                _psionic.AddRandomPsionicPower(target, true);
        }
    }
}
