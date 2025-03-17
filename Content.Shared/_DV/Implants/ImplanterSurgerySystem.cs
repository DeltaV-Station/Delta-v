using System.Linq;
using Content.Shared._Shitmed.Medical.Surgery.Steps.Parts;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Implants;

namespace Content.Shared._DV.Implants;

public sealed class ImplanterSurgerySystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, AddImplantAttemptEvent>(OnAttemptImplant);
    }

    private void OnAttemptImplant(Entity<BodyComponent> ent, ref AddImplantAttemptEvent args)
    {
        if (HasComp<ImplanterSurgerylessComponent>(args.Implanter))
            return;

        var child = _body.GetBodyChildrenOfType(ent, BodyPartType.Torso).Select(it => it.Id).FirstOrDefault();

        if (HasComp<IncisionOpenComponent>(child))
            return;

        args.Cancel();
    }
}
