using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Objectives;
using Content.Shared._DV.FeedbackOverwatch;
using Content.Shared.Mind;
using Content.Shared.Random.Helpers;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.Antag;

public sealed class NukieOperationSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IGameTiming _time = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly NukeopsRuleSystem _nukeops = default!;
    [Dependency] private readonly ObjectivesSystem _objectives = default!;
    [Dependency] private readonly SharedFeedbackOverwatchSystem _feedback = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NukieOperationComponent, AfterAntagEntitySelectedEvent>(OnAntagSelected);
        SubscribeLocalEvent<GetNukeCodePaperWriting>(OnNukeCodePaperWritingEvent);
    }

    /// <summary>
    /// This runs the automatic war declaration, distributes TC, and makes sure it only happens when it's not hostage ops.
    /// </summary>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<NukieAutoWarComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_time.CurTime < comp.AutoWarCallTime)
                continue;

            RemCompDeferred<NukieAutoWarComponent>(uid);
            var nukeops = Comp<NukeopsRuleComponent>(uid);
            nukeops.WarDeclaredTime = _time.CurTime;
            _nukeops.DistributeExtraTc((uid, nukeops));
            _chat.DispatchGlobalAnnouncement(Loc.GetString("nuke-ops-auto-war-message"),
                Loc.GetString("chat-manager-sender-announcement"),
                true, new SoundPathSpecifier("/Audio/Announcements/war.ogg"), Color.DarkRed);
        }
    }

    private void OnAntagSelected(Entity<NukieOperationComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        // Yes this is bad, but I couldn't easily find an event that would work.
        if (ent.Comp.ChosenOperation == null)
        {
            if (!_proto.TryIndex(ent.Comp.Operations, out var opProto))
                return;

            ent.Comp.ChosenOperation = _random.Pick(opProto.Weights);
        }

        if (!_mind.TryGetMind(args.Session, out var mindId, out var mind))
            return;

        if (!_proto.TryIndex(ent.Comp.ChosenOperation, out var chosenOp))
            return;

        foreach (var objectiveProto in chosenOp.OperationObjectives)
        {
            if (!_objectives.TryCreateObjective((mindId, mind), objectiveProto, out var objective))
            {
                Log.Error("Couldn't create objective for nukie: " + mindId); // This should never happen.
                continue;
            }

            _mind.AddObjective(mindId, mind, objective.Value);

            // TODO: Remove once enough feedback has been received!
            if (objectiveProto.Id == "KidnapHeadsObjective")
                _feedback.SendPopupMind(mindId, "NukieHostageRoundStartPopup");
        }
        if (chosenOp.AutoWarAfter is { } duration)
        {
            var autoWar = EnsureComp<NukieAutoWarComponent>(ent);
            autoWar.AutoWarCallTime = _time.CurTime + duration;
        }
    }

    private void OnNukeCodePaperWritingEvent(ref GetNukeCodePaperWriting ev)
    {
        // This is suspect AT BEST
        var query = EntityQueryEnumerator<NukieOperationComponent>();
        while (query.MoveNext(out _, out var nukieOperation)) // this should only loop once.
        {
            if (!_proto.TryIndex(nukieOperation.ChosenOperation, out var opProto) || opProto.NukeCodePaperOverride == null)
                continue;
            ev.ToWrite = Loc.GetString(opProto.NukeCodePaperOverride);
        }
    }
}
