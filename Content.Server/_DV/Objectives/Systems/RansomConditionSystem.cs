using Content.Server._DV.Objectives.Components;
using Content.Server.Chat.Systems;
using Content.Server.Objectives.Systems;
using Content.Shared._DV.Traitor;
using Content.Shared.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio;

namespace Content.Server._DV.Objectives.Systems;

/// <summary>
/// Makes ransom announcements for ransom objectives and mob extraction objectives.
/// </summary>
public sealed class RansomConditionSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly CodeConditionSystem _codeCondition = default!;
    [Dependency] private readonly ContractObjectiveSystem _contract = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly RansomSystem _ransom = default!;
    [Dependency] private readonly TargetObjectiveSystem _targetObjective = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateComponent, FultonedEvent>(OnFultoned);
    }

    private void OnFultoned(Entity<MobStateComponent> ent, ref FultonedEvent args)
    {
        if (!TryComp<ExtractingComponent>(ent, out var extracting))
            return;

        RemCompDeferred<ExtractingComponent>(ent);

        var ransom = _ransom.RansomEntity(ent);
        var msg = Loc.GetString("syndicate-ransom-announcement", ("hostage", ent), ("ransom", ransom));
        var sender = Loc.GetString("syndicate-ransom-announcement-sender");
        var sound = new SoundPathSpecifier("/Audio/Misc/notice1.ogg");
        var color = Color.Red;
        _chat.DispatchGlobalAnnouncement(msg, sender, playSound: true, sound, color);

        // TODO: put their inventory into the vault

        // complete the objective of the person that kidnapped them
        if (_mob.IsAlive(ent) && extracting.Mind is {} mindId && FindObjective(mindId, ent) is {} objective)
            _codeCondition.SetCompleted(objective);

        _contract.FailContracts<RansomConditionComponent>(obj => TargetEquals(obj, ent));
    }

    public EntityUid? FindObjective(Entity<MindComponent?> mind, EntityUid mob)
    {
        if (!Resolve(mind, ref mind.Comp))
            return null;

        foreach (var objective in mind.Comp.Objectives)
        {
            if (!HasComp<RansomConditionComponent>(objective) || _codeCondition.IsCompleted(objective))
                continue;

            if (TargetEquals(objective, mob))
                return objective;
        }

        return null;
    }

    private bool TargetEquals(EntityUid objective, EntityUid mob)
    {
        if (!_targetObjective.GetTarget(objective, out var target))
            return false;

        // get the actual mob targeted for the objective
        if (TryComp<MindComponent>(target, out var targetMind) && GetEntity(targetMind.OriginalOwnedEntity) is {} targetMob)
            target = targetMob;

        return mob == target;
    }
}
