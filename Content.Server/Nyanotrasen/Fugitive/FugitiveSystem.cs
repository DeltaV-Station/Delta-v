using System.Linq;
using System.Text;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Server.Ghost.Roles.Events;
using Content.Server.Traitor;
using Content.Server.Objectives;
using Content.Server.Chat.Systems;
using Content.Server.Communications;
using Content.Server.Paper;
using Content.Server.Humanoid;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Server.Ghost.Components;
using Content.Server.Roles;
using Content.Server.GameTicking;
using Content.Shared.Roles;
using Content.Shared.Movement.Systems;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Humanoid;
using Content.Shared.Random.Helpers;
using Content.Shared.Examine;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Audio;
using Robust.Shared.Utility;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Server.GameObjects;
using Robust.Server.Audio;
using static Content.Shared.Examine.ExamineSystemShared;

namespace Content.Server.Fugitive
{
    public sealed class FugitiveSystem : EntitySystem
    {
        private const string FugitiveRole = "Fugitive";
        private const string EscapeObjective = "EscapeShuttleObjectiveFugitive";

        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly HumanoidAppearanceSystem _humanoidSystem = default!;
        [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly PaperSystem _paperSystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly StunSystem _stun = default!;
        [Dependency] private readonly AudioSystem _audioSystem = default!;
        [Dependency] private readonly SharedMindSystem _mindSystem = default!;
        [Dependency] private readonly MetaDataSystem _metaData = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<FugitiveComponent, GhostRoleSpawnerUsedEvent>(OnSpawned);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            foreach (var (cd, _) in EntityQuery<FugitiveCountdownComponent, FugitiveComponent>())
            {
                if (cd.AnnounceTime != null && _timing.CurTime > cd.AnnounceTime)
                {
                    _chat.DispatchGlobalAnnouncement(Loc.GetString("station-event-fugitive-hunt-announcement"), sender: Loc.GetString("fugitive-announcement-GALPOL"), colorOverride: Color.Yellow);

                    foreach (var console in EntityQuery<CommunicationsConsoleComponent>())
                    {
                        if (HasComp<GhostComponent>(console.Owner))
                            continue;

                        var paperEnt = Spawn("Paper", Transform(console.Owner).Coordinates);
                        var fugireport = Loc.GetString("fugi-report-ent-name", ("name", cd.Owner));
                        _metaData.SetEntityName(paperEnt, fugireport);

                        if (!TryComp<PaperComponent>(paperEnt, out var paper))
                            continue;

                        var report = GenerateFugiReport(cd.Owner);

                        _paperSystem.SetContent(paperEnt, report.ToMarkup(), paper);
                    }

                    RemCompDeferred<FugitiveCountdownComponent>(cd.Owner);
                }
            }
        }

        private void OnSpawned(EntityUid uid, FugitiveComponent component, GhostRoleSpawnerUsedEvent args)
        {
            if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
                return;

            var pref = HumanoidCharacterProfile.Random();
            var species = pref.Species;
            _humanoidSystem.SetSpecies(uid, species, true, humanoid);
            _humanoidSystem.LoadProfile(uid, pref);

            if (TryComp<FugitiveCountdownComponent>(uid, out var cd))
                cd.AnnounceTime = _timing.CurTime + cd.AnnounceCD;

            _popupSystem.PopupEntity(Loc.GetString("fugitive-spawn", ("name", uid)), uid,
            Filter.Pvs(uid).RemoveWhereAttachedEntity(entity => !ExamineSystemShared.InRangeUnOccluded(uid, entity, ExamineRange, null)), true, Shared.Popups.PopupType.LargeCaution);

            _stun.TryParalyze(uid, TimeSpan.FromSeconds(2), false);
            _audioSystem.PlayPvs(component.SpawnSoundPath, uid, AudioParams.Default.WithVolume(-6f));
        }

        private FormattedMessage GenerateFugiReport(EntityUid uid)
        {
            FormattedMessage report = new();
            report.AddMarkup(Loc.GetString("fugi-report-title", ("name", uid)));
            report.PushNewline();
            report.PushNewline();
            report.AddMarkup(Loc.GetString("fugitive-report-first-line", ("name", uid)));
            report.PushNewline();
            report.PushNewline();


            if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoidComponent) ||
                !_prototypeManager.TryIndex<SpeciesPrototype>(humanoidComponent.Species, out var species))
            {
                report.AddMarkup(Loc.GetString("fugitive-report-inhuman", ("name", uid)));
                return report;
            }

            report.AddMarkup(Loc.GetString("fugitive-report-morphotype", ("species", Loc.GetString(species.Name))));
            report.PushNewline();
            report.AddMarkup(Loc.GetString("fugitive-report-age", ("age", humanoidComponent.Age)));
            report.PushNewline();

            string sexLine = string.Empty;
            sexLine += humanoidComponent.Sex switch
            {
                Sex.Male => Loc.GetString("fugitive-report-sex-m"),
                Sex.Female => Loc.GetString("fugitive-report-sex-f"),
                _ => Loc.GetString("fugitive-report-sex-n")
            };

            report.AddMarkup(sexLine);

            if (TryComp<PhysicsComponent>(uid, out var physics))
            {
                report.PushNewline();
                report.AddMarkup(Loc.GetString("fugitive-report-weight", ("weight", Math.Round(physics.FixturesMass))));
            }
            report.PushNewline();
            report.PushNewline();
            report.AddMarkup(Loc.GetString("fugitive-report-last-line"));

            return report;
        }
    }
}
