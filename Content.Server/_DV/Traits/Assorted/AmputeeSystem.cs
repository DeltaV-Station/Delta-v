using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Robust.Server.GameObjects;

namespace Content.Server._DV.Traits.Assorted;

public sealed class AmputeeSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AmputeeComponent, MapInitEvent>(OnMapInit);
    }

    // Logic here taken from Den at https://github.com/TheDenSS14/TheDen/blob/d6f85a10fccf0f282981438e55b808d7ece73ad9/Content.Server/Traits/TraitSystem.Functions.cs
    private void OnMapInit(Entity<AmputeeComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp(ent, out BodyComponent? body) || !TryComp(ent, out TransformComponent? xform))
            return;

        var root = _body.GetRootPartOrNull(ent, body);
        if (root is null)
            return;

        var parts = _body.GetBodyChildrenOfType(ent, ent.Comp.RemoveBodyPart, body);
        foreach (var part in parts)
        {
            var partComp = part.Component;
            if (partComp.Symmetry != ent.Comp.PartSymmetry)
                continue;

            foreach (var child in _body.GetBodyPartChildren(part.Id, part.Component))
            {
                QueueDel(child.Id);
            }

            _transform.AttachToGridOrMap(part.Id);
            QueueDel(part.Id);

            // apparently chopping off limbs makes people bleed a lot. Who would have guessed?
            _bloodstream.TryModifyBleedAmount(ent.Owner, -10f);

            // goes unused for the purposes of the arm amputee traits, but might as well keep it in
            if (ent.Comp.ProtoId is null || ent.Comp.SlotId == null)
                continue;

            var newLimb = SpawnAtPosition(ent.Comp.ProtoId, xform.Coordinates);
            if (TryComp<BodyPartComponent>(newLimb, out var limbComp) && limbComp.Symmetry == ent.Comp.PartSymmetry)
                _body.AttachPart(root.Value.Entity, ent.Comp.SlotId, newLimb, root.Value.BodyPart, limbComp);
        }
    }
}
