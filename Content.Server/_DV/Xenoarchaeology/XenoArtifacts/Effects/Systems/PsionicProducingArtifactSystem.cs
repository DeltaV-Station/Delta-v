using Content.Server._DV.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Server.Psionics;

public sealed class PsionicProducingArtifactSystem : EntitySystem
{
    [Dependency] private readonly ArtifactSystem _artifact = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PsionicsSystem _psionics = default!;

    public const string NodeDataPsionicAmount = "nodeDataPsionicAmount";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PsionicProducingArtifactComponent, ArtifactActivatedEvent>(OnActivated);
    }

    private void OnActivated(Entity<PsionicProducingArtifactComponent> ent, ref ArtifactActivatedEvent args)
    {
        var (uid, comp) = ent;
        if (!_artifact.TryGetNodeData(uid, NodeDataPsionicAmount, out int amount))
            amount = 0;

        if (amount >= comp.Limit)
            return;

        var coords = Transform(uid).Coordinates;
        foreach (var target in _lookup.GetEntitiesInRange<PotentialPsionicComponent>(coords, comp.Range))
        {
            _psionics.TryMakePsionic(target);
        }

        _artifact.SetNodeData(uid, NodeDataPsionicAmount, amount + 1);
    }
}
