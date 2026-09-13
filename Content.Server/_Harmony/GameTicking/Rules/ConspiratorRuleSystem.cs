using Content.Server._Harmony.GameTicking.Rules.Components;
using Content.Server.Antag;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Roles;
using Content.Server.Administration.Logs;
using Content.Server.Voting.Managers;
using Content.Server.Voting;
using Content.Server.Objectives;
using Content.Server.Polymorph.Components;
using Content.Shared._Harmony.Conspirators.Components;
using Content.Shared._Harmony.Roles.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
using Content.Shared.IdentityManagement;
using Content.Shared.Random.Helpers;
using Content.Shared._DV.CCVars;
using Content.Shared.Database;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Robust.Shared.Timing;
using Robust.Shared.Enums;
using Robust.Shared.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server._Harmony.GameTicking.Rules;

public sealed class ConspiratorRuleSystem : GameRuleSystem<ConspiratorRuleComponent>
{
    // [Dependency] private readonly AntagSelectionSystem _antag = default!; // Delta V - Never used
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IVoteManager _votes = default!;
    
    
    private TimeSpan _objectiveVoteTimer = default!;
    private TimeSpan _objectiveVoteDelay = default!;
    private TimeSpan _leaderVoteTimer = default!;
    private TimeSpan _leaderVoteDelay = default!;

    private readonly SoundSpecifier _conspiratorBriefing = new SoundPathSpecifier("/Audio/_Harmony/Misc/conspirator_greeting.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConspiratorRoleComponent, GetBriefingEvent>(OnGetBriefing);
        SubscribeLocalEvent<ConspiratorRuleComponent, AfterAntagEntitySelectedEvent>(OnAntagSelected);

        // deltav additions, conspirators v2
        Subs.CVar(_config,
            DCCVars.ConspiratorObjectiveVoteTimer,
            value => _objectiveVoteTimer = TimeSpan.FromSeconds(value),
            true);
        Subs.CVar(_config,
            DCCVars.ConspiratorObjectiveVoteDelayTimer,
            value => _objectiveVoteDelay = TimeSpan.FromSeconds(value),
            true);
        Subs.CVar(_config,
            DCCVars.ConspiratorLeaderVoteTimer,
            value => _leaderVoteTimer = TimeSpan.FromSeconds(value),
            true);
        Subs.CVar(_config,
            DCCVars.ConspiratorLeaderVoteDelayTimer,
            value => _leaderVoteDelay = TimeSpan.FromSeconds(value),
            true);
        // deltav additions, conspirators v2
    }
    
    
    /* DeltaV - removed custom round end text in favor of individually displayed objective summaries

    protected override void AppendRoundEndText(EntityUid uid, ConspiratorRuleComponent component,GameRuleComponent gameRule,ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        var sessionData = _antag.GetAntagIdentifiers(uid);
        args.AddLine(Loc.GetString("conspirator-count", ("count", sessionData.Count)));
        foreach (var (_, data, name) in sessionData)
        {
            args.AddLine(Loc.GetString("conspirator-name-user",
                ("name", name),
                ("username", data.UserName)));
        }

        if (!_proto.TryIndex(component.Objective, out var objectiveProto))
            return;

        args.AddLine(Loc.GetString("conspirator-objective", ("objective", objectiveProto.Name)));
    }
    */

    private void OnGetBriefing(Entity<ConspiratorRoleComponent> ent, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("conspirator-identities"));

        var conspirators = AllEntityQuery<ConspiratorComponent>();
        while (conspirators.MoveNext(out var id, out _))
        {
            args.Append(Loc.GetString("conspirator-name", ("name", Name(id))));
        }

        args.Append(Loc.GetString("conspirator-radio-implant"));
    }

    
    private void OnAntagSelected(Entity<ConspiratorRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        if (!_mind.TryGetMind(args.Session, out var mindId, out var mind))
            return;
        
        /*
        if (ent.Comp.Objective is null){
            if (GetRandomObjectivePrototype(ent.Comp, out var objectiveProtoId))
               ent.Comp.Objective = objectiveProtoId;
        }
        */
        
        if (ent.Comp.Objective is not null)
            _mind.TryAddObjective(mindId, mind, ent.Comp.Objective);

    }
    

    /* deltaV conspirators v2, now uneeded so.
    private bool GetRandomObjectivePrototype(ConspiratorRuleComponent comp, [NotNullWhen(true)] out EntProtoId? objectiveProto)
    {
        objectiveProto = null;

        if (!_proto.TryIndex(comp.ObjectiveGroup, out var group))
            return false;

        var objectives = group.Weights.ShallowClone();
        while (_random.TryPickAndTake(objectives, out var proto))
        {
            objectiveProto = proto!;
            return true;
        }

        return false;
    } 
    */

    //delta V addition - conspirators vote system. i am too scared to make generic systems so woe, copy paste code be upon ye
    protected override void Started(EntityUid uid, ConspiratorRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        component.ConspiratorLeaderVoteTimer = _timing.CurTime + _leaderVoteDelay;
        component.ConspiratorObjectiveVoteTimer = _timing.CurTime + _objectiveVoteDelay;
    }

    protected override void ActiveTick(EntityUid uid, ConspiratorRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        if (component.ConspiratorLeaderVoteTimer is { } _objectiveVoteTimer && _timing.CurTime >= _objectiveVoteTimer)
        {
            component.ConspiratorLeaderVoteTimer = null;
            ConspiratorObjectiveVote(component);
        } else if (component.ConspiratorObjectiveVoteTimer is { } _leaderVoteTimer && _timing.CurTime >= _leaderVoteTimer) 
        {
            component.ConspiratorObjectiveVoteTimer = null;
            ConspiratorLeaderVote();
        }
    }

    private void ConspiratorLeaderVote()
    {
        // If there's already an entity with ConspiratorLeader, don't hold a vote. This allows admins to add the conspirators rule a 2nd time
        // in the case that there is only one cultist and they've been chosen as the leader already.
        if (EntityQuery<ConspiratorLeaderComponent>().Any())
        {
            _adminLogger.Add(LogType.Vote, LogImpact.Medium,
                $"conspirator leader already exists. Cancelling leader vote.");
            return;
        }
        
        var conspirators = new List<(string, EntityUid)>();
        var conspiratorsQuery = EntityQueryEnumerator<ConspiratorComponent, MetaDataComponent>();

        while (conspiratorsQuery.MoveNext(out var conspirator, out _, out var metadata))
        {
            var playerInfo = metadata.EntityName;
            if (TryComp<PolymorphedEntityComponent>(conspirator, out var polyComp) && polyComp.Parent.HasValue) // If the cultist is polymorphed, we use the original entity instead and hope that they'll polymorph back eventually
                conspirators.Add((playerInfo, polyComp.Parent.Value));
            else
                conspirators.Add((playerInfo, conspirator));
        }

        var options = new VoteOptions
        {
            Title = Loc.GetString("conspirator-vote-leader-title"),
            InitiatorText = Loc.GetString("conspirators-vote-leader-initiator"),
            Duration = _leaderVoteTimer,
            VoterEligibility = VoteManager.VoterEligibility.Conspirators
        };

        // If there are no conspirators, don't hold a vote, or the server will crash.
        if (conspirators.Count == 0)
        {
            Log.Warning($"There are no conspirators present for the leader vote. Voting is cancelled to prevent the server crashing.");
            _adminLogger.Add(LogType.Vote, LogImpact.Extreme, $"There are no conspirators for the leader vote. Leader vote is cancelled to prevent the server crashing.");
            return;
        }

        foreach (var (name, ent) in conspirators)
        {
            options.Options.Add((Loc.GetString(name), ent));
        }

        // If somehow there are conspirators but no options, still don't hold a vote.
        // Holding a vote with zero options crashes the server.
        if (options.Options.Count == 0)
        {
            Log.Warning($"There are {conspirators.Count} conspirators but no options for the leader vote. Voting is cancelled to prevent the server crashing.");
            _adminLogger.Add(LogType.Vote, LogImpact.Extreme, $"There are {conspirators.Count} conspirators but no options for the leader vote. Steward vote is cancelled to prevent the server crashing.");
            return;
        }

        var vote = _votes.CreateVote(options);

        vote.OnFinished += (_, args) =>
        {
            EntityUid picked;
            if (args.Winner == null)
            {
                picked = (EntityUid)_random.Pick(args.Winners);
            }
            else
            {
                picked = (EntityUid)args.Winner;
            }
            //changing the icon of the head conspirator
            EnsureComp<ConspiratorLeaderComponent>(picked);
            _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"conspirators leadership vote finished: {Identity.Entity(picked, EntityManager)} is now leader.");
        };
    }

    //summary -> voting system for the conspirators objective 
    private void ConspiratorObjectiveVote(ConspiratorRuleComponent component)
    {
        //getting all conspirators and checking if they already have a objective
        var conspirators = new List<EntityUid>();
        var conspiratorsQuery = EntityQueryEnumerator<ConspiratorComponent>();

        while (conspiratorsQuery.MoveNext(out var conspirator, out _))
        {
            if (TryComp<PolymorphedEntityComponent>(conspirator, out var polyComp) && polyComp.Parent.HasValue) // If the cultist is polymorphed, we use the original entity instead and hope that they'll polymorph back eventually
                conspirators.Add((polyComp.Parent.Value));
            else
                conspirators.Add((conspirator));
        }
        
        // If there are no conspirators, don't hold a vote, or the server will crash.
        if (conspirators.Count == 0)
        {
            Log.Warning($"There are no conspirators present for the objective vote. Voting is cancelled");
            _adminLogger.Add(LogType.Vote, LogImpact.Extreme, $"There are no conspirators for the objective vote. objective vote is cancelled");
            return;
        }

        var options = new VoteOptions
        {
            Title = Loc.GetString("conspirator-vote-objective-title"),
            InitiatorText = Loc.GetString("conspirators-vote-leader-initiator"),
            Duration = _objectiveVoteTimer,
            VoterEligibility = VoteManager.VoterEligibility.Conspirators
        };

        //dumb array go! from here use it to add the options, will have to add the new objectives to this.
        int ObjectiveArrayNumber = 0;
        string[] ConspiratorObjectiveIds = ["ConspiratorBusinessObjective","ConspiratorUsurpObjective","ConspiratorHordeObjective","ConspiratorDistrustObjective","ConspiratorVigilanteObjective","ConspiratorFreedomObjective","ConspiratorNukeObjective","ConspiratorDangerObjective","ConspiratorFreeObjective"];
        string[] ConspiratorObjectiveNames = ["Set up a business outside Nanotrasen.","Become the true leaders of the station.","Build a horde of valuables.","Brew distrust and hatred.","Enforce the laws secuirty can not.","Free the station of access.","Steal the nuke disk","Arm yourselves","Make your own conspiracy."];
        
        foreach (string objective in ConspiratorObjectiveNames)
        {
            options.Options.Add((Loc.GetString(objective),ConspiratorObjectiveIds[ObjectiveArrayNumber]));
            ObjectiveArrayNumber++;
        }

        // If somehow there are cultists but no options, still don't hold a vote.
        // Holding a vote with zero options crashes the server.
        if (options.Options.Count == 0)
        {
            Log.Warning($"There are {conspirators.Count} conspirators but no options for the leader vote. Voting is cancelled to prevent the server crashing.");
            _adminLogger.Add(LogType.Vote, LogImpact.Extreme, $"There are {conspirators.Count} conspirators but no options for the leader vote. Steward vote is cancelled to prevent the server crashing.");
            return;
        }

        var vote = _votes.CreateVote(options);

        vote.OnFinished += (_, args) =>
        {
            string picked;
            if (args.Winner == null)
            {
                picked = (string)_random.Pick(args.Winners);
            }
            else
            {
                picked = (string)args.Winner;
            }

            //add an objective for each member of the conspiracy, skip if it cant get them
            foreach (EntityUid ent in conspirators){
                _mind.TryGetMind(ent, out var mindId, out var mind);
                if (mind == null){
                    continue;
                }
                _mind.TryAddObjective(mindId, mind, picked);
            }
            component.Objective = picked;
            _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"conspirators objective vote finished: {picked} is the objective.");
        };
    }
    
    // deltav additions, conspirators v2
}
