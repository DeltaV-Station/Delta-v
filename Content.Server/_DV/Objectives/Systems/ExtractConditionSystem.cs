using Content.Server._DV.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared._DV.Traitor;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._DV.Objectives.Systems;

public sealed class ExtractConditionSystem : EntitySystem
{
    [Dependency] private readonly CodeConditionSystem _codeCondition = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly ContractObjectiveSystem _contract = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExtractConditionComponent, ObjectiveAssignedEvent>(OnAssigned);
        SubscribeLocalEvent<ExtractConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);

        SubscribeLocalEvent<StealTargetComponent, FultonedEvent>(OnFultoned);
    }

    /// start checks of target acceptability, and generation of start values.
    private void OnAssigned(Entity<ExtractConditionComponent> ent, ref ObjectiveAssignedEvent args)
    {
        if (args.Cancelled || !ent.Comp.VerifyMapExistence || args.Mind.OwnedEntity is not {} mob)
            return;

        // very important: only check the current map, so syndie vault doesn't count as existing
        var map = Transform(mob).MapID;

        var found = false;
        var query = EntityQueryEnumerator<StealTargetComponent, TransformComponent>();
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

        var description = Loc.GetString(ent.Comp.DescriptionText, ("itemName", localizedName));

        _meta.SetEntityName(ent, title, args.Meta);
        _meta.SetEntityDescription(ent, description, args.Meta);
        _objectives.SetIcon(ent, group.Sprite, args.Objective);
    }

    private void OnFultoned(Entity<StealTargetComponent> ent, ref FultonedEvent args)
    {
        // don't touch objectives for salv fultons, return early
        if (!TryComp<ExtractingComponent>(ent, out var extracting))
            return;

        RemCompDeferred<ExtractingComponent>(ent);

        // complete the objective of the person that extracted it
        if (extracting.Mind is {} mindId && FindObjective(mindId, (ent, ent.Comp)) is {} objective)
            _codeCondition.SetCompleted(objective);

        // fail every other contract for the same thing
        var group = ent.Comp.StealGroup;
        _contract.FailContracts<ExtractConditionComponent>(obj => obj.Comp.StealGroup == group);
    }

    /// <summary>
    /// Find an objective that wants an item, or null if it isn't wanted.
    /// </summary>
    public EntityUid? FindObjective(Entity<MindComponent?> mind, Entity<StealTargetComponent?> item)
    {
        if (!Resolve(mind, ref mind.Comp) || !Resolve(item, ref item.Comp, false))
            return null;

        var group = item.Comp.StealGroup;
        foreach (var objective in mind.Comp.Objectives)
        {
            if (!TryComp<ExtractConditionComponent>(objective, out var comp))
                continue;

            // skip already completed objectives
            if (_codeCondition.IsCompleted(objective))
                continue;

            if (comp.StealGroup == group)
                return objective;
        }

        return null;
    }
}
