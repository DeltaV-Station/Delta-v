using Content.Server._DV.Objectives.Components;
using Content.Server.Objectives.Components.Targets;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._DV.Objectives.Systems;

public sealed class ExtractConditionSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExtractConditionComponent, ObjectiveAssignedEvent>(OnAssigned);
        SubscribeLocalEvent<ExtractConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
    }

    /// start checks of target acceptability, and generation of start values.
    private void OnAssigned(Entity<ExtractConditionComponent> ent, ref ObjectiveAssignedEvent args)
    {
        if (args.Cancelled || !ent.Comp.VerifyMapExistence)
            return;

        var map = Transform(args.Mind.OwnedEntity).MapID;

        var found = false;
        var query = EntityQueryEnumerator<ExtractTargetComponent, TransformComponent>();
        var group = ent.Comp.StealGroup;
        while (query.MoveNext(out var target, out var xform))
        {
            if (xform.MapID != map || target.StealGroup != group)
                continue;

            found = true;
            break;
        }

        args.Cancelled = !found;
    }

    //Set the visual, name, icon for the objective.
    private void OnAfterAssign(Entity<ExtractConditionComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
        var group = _proto.Index(ent.Comp.StealGroup);
        string localizedName = Loc.GetString(group.Name);

        var title = ent.Comp.OwnerText == null
            ? Loc.GetString(ent.Comp.ObjectiveNoOwnerText, ("itemName", localizedName))
            : Loc.GetString(ent.Comp.ObjectiveText, ("owner", Loc.GetString(ent.Comp.OwnerText)), ("itemName", localizedName));

        var description = ent.Comp.CollectionSize > 1
            ? Loc.GetString(ent.Comp.DescriptionMultiplyText, ("itemName", localizedName), ("count", ent.Comp.CollectionSize))
            : Loc.GetString(ent.Comp.DescriptionText, ("itemName", localizedName));

        _meta.SetEntityName(ent, title, args.Meta);
        _meta.SetEntityDescription(ent, description, args.Meta);
        _objectives.SetIcon(ent, group.Sprite, args.Objective);
    }

    /// <summary>
    /// Find an objective that wants an item, or null if it isn't wanted.
    /// </summary>
    public EntityUid? FindObjective(Entity<MindComponent?> mind, Entity<StealTargetComponent?> item)
    {
        if (!Resolve(mind, ref mind.Comp) || !Resolve(item, ref item.Comp, false))
            return null;

        var group = item.Comp.Group;
        foreach (var objective in mind.Comp.Objectives)
        {
            if (!TryComp<ExtractConditionComponent>(objective, out var comp))
                continue;

            // skip already completed objectives
            if (TryComp<CodeConditionComponent>(objective, out var code) && code.Complete)
                continue;

            if (comp.StealGroup == group)
                return objective;
        }

        return null;
    }
}
