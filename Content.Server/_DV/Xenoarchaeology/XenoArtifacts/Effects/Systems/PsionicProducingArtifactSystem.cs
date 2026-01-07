using System.Linq;
using Content.Server._DV.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Psionics;
using Content.Shared.Xenoarchaeology.Artifact;
using Content.Shared.Xenoarchaeology.Artifact.Components;

namespace Content.Server._DV.Xenoarchaeology.XenoArtifacts.Effects.Systems;
public sealed class PsionicProducingArtifactSystem : EntitySystem
{
    [Dependency] private readonly SharedXenoArtifactSystem _artifact = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PsionicsSystem _psionics = default!;

    public const string NodeDataPsionicAmount = "nodeDataPsionicAmount";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PsionicProducingArtifactComponent, XenoArtifactActivatedEvent>(OnActivated);
    }

    private void OnActivated(Entity<PsionicProducingArtifactComponent> ent, ref XenoArtifactActivatedEvent args)
    {
        var (uid, comp) = ent;

        // Resolve the artifact entity from the node
        if (!TryComp<XenoArtifactComponent>(uid, out var artifactComp))
            return;

        var artifactEntity = new Entity<XenoArtifactComponent>(uid, artifactComp);

        // Pick first active node
        var node = _artifact.GetActiveNodes(artifactEntity).FirstOrDefault();

        // Track psionic usage using ConsumedResearchValue
        var currentAmount = _artifact.GetResearchValue(node);

        if (currentAmount >= comp.Limit)
            return;

        var coords = Transform(uid).Coordinates;

        foreach (var target in _lookup.GetEntitiesInRange<PotentialPsionicComponent>(coords, comp.Range))
        {
            _psionics.TryMakePsionic(target);
        }

        // Update node usage
        _artifact.SetConsumedResearchValue(node, currentAmount + 1);
    }

}
