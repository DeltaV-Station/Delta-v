using System.Collections.Generic;
using System.Linq;
using Content.Server.Palmtree.Surgery;
using Content.Server.Body.Systems;
using Content.Server.Popups;
using Content.Server.Mind;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Palmtree.Surgery;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
//using Content.Shared.Players;
using Content.Shared.Standing;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Bed.Sleep;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

// It's all very crude at the moment, but it actually works now.

/*
 * PalmtreeMed surgery by TropicalOwl
 * The current code is grotesque, stuff is hardcoded and it looks nothing like how I want it to actually look like in the final version.
 * Additionally please do note this was programmed by someone who had very little contact with C# prior to this, if anyone want to make improvements or rework this, don't need to ask for permission, I'm not gonna pretend I know what I'm doing.
 *
 * This is also a port from a non-ee code, so stuff might break as well.
 * You've been warned
 *
 * On the plus side, it's VERY easy to remove this from the game if someone comes and makes a better system.
 */



namespace Content.Server.Palmtree.Surgery.SurgerySystem
{
    public class PSurgerySystem : SharedSurgerySystem
    {
        // These procedures can be prototypes later, for now I'll hardcode them because I have no idea how to do it otherwise.
        // Remind me to try and make them prototypes later, please!
        Dictionary<string, string[]> procedures = new Dictionary<string, string[]>
        {
            {"TendWounds", new string[]{"scalpel"}},
            {"FilterBlood", new string[]{"scalpel", "retractor", "saw"}},
            {"SawBones", new string[]{"scalpel", "retractor"}}, // This comes before letting people saw bones
            {"BrainTransfer", new string[]{"scalpel", "retractor", "saw"}},
            {"ReduceRotting", new string[]{"scalpel", "retractor"}},
            {"InsertAugment", new string[]{"scapel", "retractor", "saw"}}, // This procedure is used for augments and implants
            {"RemoveAugment", new string[]{"scalpel", "retractor", "saw", "drill"}} // This procedure removes a person's augments, breaking them
        };

        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly SharedMindSystem _sharedmind = default!;
        [Dependency] private readonly MindSystem _mind = default!;
        [Dependency] private readonly StandingStateSystem _standing = default!;
        //[Dependency] private readonly IPlayerManager _player = default!;
        [Dependency] private readonly SharedRottingSystem _rot = default!;
        [Dependency] private readonly BloodstreamSystem _blood = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly MobStateSystem _mobState = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PSurgeryToolComponent, AfterInteractEvent>(OnAfterInteract);
            SubscribeLocalEvent<PSurgeryToolComponent, SurgeryDoAfterEvent>(OnProcedureFinished);
        }
        private void OnProcedureFinished(EntityUid uid, PSurgeryToolComponent tool, SurgeryDoAfterEvent args)
        {
            if (args.Cancelled || args.Target == null || !TryComp(args.Target, out PPatientComponent? patient)) return;
            if (patient.procedures.Count == 0)
            {
                if (tool.kind == "scalpel")
                {
                    _popupSystem.PopupEntity("You successfully perform an incision!", args.User, PopupType.Small);
                    patient.procedures.Add("scalpel");
                    _damageableSystem.TryChangeDamage(args.Target, tool.damageOnUse, true, origin: uid);
                    _blood.TryModifyBleedAmount((EntityUid) args.Target, tool.bleedAmountOnUse);
                    _audio.PlayPvs(tool.audioEnd, args.User);
                }
                else
                {
                    _popupSystem.PopupEntity("Perform an incision first!", args.User, PopupType.Small);
                }
            }
            else
            { // Procedure checks go here
                bool repeatableProcedure = false; // If it is repeatable it won't add to the "crafting" of the surgery
                bool damageOnFinish = true; // Check if it will damage the target by the damage specified in the prototypes
                bool failedProcedure = false; // Check if it has failed.
                switch(tool.kind)
                {
                    case "scalpel":
                        _popupSystem.PopupEntity("You perform an incision!", args.User, PopupType.Small);
                        break;
                    case "hemostat":
                        repeatableProcedure = true; // You can pinch people over and over
                        _popupSystem.PopupEntity("You clamp the patient's bleeders!", args.User, PopupType.Small);
                        break;
                    case "retractor":
                        _popupSystem.PopupEntity("You retract the patient's skin!", args.User, PopupType.Small);
                        break;
                    case "saw":
                        if (patient.procedures.SequenceEqual(procedures["SawBones"]))
                        {
                            _popupSystem.PopupEntity("You saw the patient's bones!", args.User, PopupType.Small);
                        }
                        else
                        {
                            _popupSystem.PopupEntity("You can't saw your patient's bones right now!", args.User, PopupType.Small);
                            failedProcedure = true;
                        }
                        break;
                    case "cautery":
                        patient.procedures.Clear();
                        repeatableProcedure = true; // You can just burn people over and over with a cautery, yeah.
                        break;
                    case "brainlink":
                        if (patient.procedures.SequenceEqual(procedures["BrainTransfer"]))
                        {
                            bool targetHasMind = _sharedmind.TryGetMind((EntityUid) args.Target, out var targetMindId, out var targetMind);
                            if (targetHasMind)
                            {
                                _mind.TransferTo(targetMindId, uid, mind: targetMind);
                            }
                            repeatableProcedure = true;
                        }
                        else
                        {
                            failedProcedure = true;
                            _popupSystem.PopupEntity("You can't take the patient's mind right now.", args.User, PopupType.Small);
                        }
                        break;
                    case "phantomlink": // Upgraded version of the ghost shell, can only be admin spawned
                        if (patient.procedures.SequenceEqual(procedures["BrainTransfer"]))
                        {
                            bool targetHasMind = _sharedmind.TryGetMind((EntityUid) args.Target, out var targetMindId, out var targetMind);
                            bool deviceHasMind = _sharedmind.TryGetMind(uid, out var deviceMindId, out var deviceMind);
                            if (targetHasMind && deviceHasMind)
                            {
                                failedProcedure = true;
                                _popupSystem.PopupEntity("You can't take the patient's mind right now, your link is occupied.", args.User, PopupType.Small);
                            }
                            else
                            {
                                if (targetHasMind && !deviceHasMind)
                                {
                                    _mind.TransferTo(targetMindId, uid, mind: targetMind);
                                }
                                if (deviceHasMind && !targetHasMind)
                                {
                                    _mind.TransferTo(deviceMindId, args.Target, mind: deviceMind);
                                }
                                repeatableProcedure = true;
                            }
                        }
                        else
                        {
                            failedProcedure = true;
                            _popupSystem.PopupEntity("You can't take the patient's mind right now.", args.User, PopupType.Small);
                        }
                        break;
                    case "bloodfilter":
                        if (patient.procedures.Contains("retractor"))
                        {
                            _popupSystem.PopupEntity("You filter the contaminants from the subject's blood.", args.User, PopupType.Small);
                            repeatableProcedure = true;
                        }
                        else
                        {
                            _popupSystem.PopupEntity("You can't filter the patient's blood right now.", args.User, PopupType.Small);
                            failedProcedure = true;
                        }
                        break;
                    case "antirot":
                        if (patient.procedures.Contains("retractor"))
                        {
                            _popupSystem.PopupEntity("You spray the anti-rot on the patient.", args.User, PopupType.Small);
                            _rot.ReduceAccumulator((EntityUid) args.Target, TimeSpan.FromSeconds(120));
                            repeatableProcedure = true;
                        }
                        else
                        {
                            _popupSystem.PopupEntity("You can't use the spray right now.", args.User, PopupType.Small);
                            failedProcedure = true;
                        }
                        break;
                    default:
                        _popupSystem.PopupEntity("If you see this, contact TropicalOwl and tell which tool you used when seeing this, this is an error message.", args.User, PopupType.Small);
                        break;
                }
                if (failedProcedure)
                {
                    return;
                }
                if (!repeatableProcedure)
                {
                    patient.procedures.Add(tool.kind);
                }
                if (damageOnFinish)
                {
                    _damageableSystem.TryChangeDamage(args.Target, tool.damageOnUse, true, origin: uid);
                    _blood.TryModifyBleedAmount((EntityUid) args.Target, tool.bleedAmountOnUse);
                }
                _audio.PlayPvs(tool.audioEnd, args.User);
            }
        }
        private void OnAfterInteract(EntityUid uid, PSurgeryToolComponent tool, AfterInteractEvent args) // Turn this into FTL strings later
        {
            bool patientHasState = TryComp(args.Target, out MobStateComponent? patientState);
            if (!args.CanReach || args.Target == null) // We already check if target is null, it's okay to perform direct conversion to non-nullable
            {
                return;
            }
            if (!TryComp(args.Target, out PPatientComponent? patient) && patientHasState)
            {
                _popupSystem.PopupEntity("You cannot perform surgery on this!", args.User, PopupType.Small);
                return;
            }
            if (!_standing.IsDown((EntityUid) args.Target))
            {
                _popupSystem.PopupEntity("The patient must be laying down and asleep!", args.User, PopupType.Small);
                return;
            }
            if (!TryComp(args.Target, out SleepingComponent? sleep) && !_mobState.IsIncapacitated((EntityUid) args.Target, patientState))
            {
                _popupSystem.PopupEntity("The patient must be asleep!", args.User, PopupType.Small);
                return;
            }
            if (args.User == args.Target)
            {
                _popupSystem.PopupEntity("You cannot operate yourself!", args.User, PopupType.Small);
                return;
            }
            var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, tool.useDelay, new SurgeryDoAfterEvent(), uid, target: args.Target)
            {
                NeedHand = true,
                //BreakOnMove = true,
                //BreakOnWeightlessMove = true,
            };
            _audio.PlayPvs(tool.audioStart, args.User);
            _doAfter.TryStartDoAfter(doAfterEventArgs);
        }
    }
}
