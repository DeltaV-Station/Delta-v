using Robust.Shared.Prototypes;
using Content.Server.Abilities.Psionics;
using Content.Server.Radio.Components;
using Content.Server.Radio.EntitySystems;
using Content.Server.StationEvents.Events;
using Content.Server.NPC.Events;
using Content.Server.NPC.Systems;
using Content.Server.NPC.Prototypes;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Radio;
using Content.Shared.Interaction;

namespace Content.Server.Research.SophicScribe
{
    public sealed partial class SophicScribeSystem : EntitySystem
    {
        [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
        [Dependency] private readonly RadioSystem _radioSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly NPCConversationSystem _conversationSystem = default!;

        private readonly ISawmill _sawmill = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<SophicScribeComponent, NPCConversationGetGlimmerEvent>(OnGetGlimmer);
            SubscribeLocalEvent<GlimmerEventEndedEvent>(OnGlimmerEventEnded);
        }

        private void OnGetGlimmer(EntityUid uid, SophicScribeComponent component, NPCConversationGetGlimmerEvent args)
        {
            if (args.Text == null)
            {
                _sawmill.Error($"{ToPrettyString(uid)} heard a glimmer reading prompt but has no text for it.");
                return;
            }

            var tier = _glimmerSystem.GetGlimmerTier() switch
            {
                GlimmerTier.Minimal => Loc.GetString("glimmer-reading-minimal"),
                GlimmerTier.Low => Loc.GetString("glimmer-reading-low"),
                GlimmerTier.Moderate => Loc.GetString("glimmer-reading-moderate"),
                GlimmerTier.High => Loc.GetString("glimmer-reading-high"),
                GlimmerTier.Dangerous => Loc.GetString("glimmer-reading-dangerous"),
                _ => Loc.GetString("glimmer-reading-critical"),
            };

            var glimmerReadingText = Loc.GetString(args.Text,
                ("glimmer", _glimmerSystem.Glimmer), ("tier", tier));

            var response = new NPCResponse(glimmerReadingText);
            _conversationSystem.QueueResponse(uid, response);
        }

        private void OnGlimmerEventEnded(GlimmerEventEndedEvent args)
            }
        }
    }

    public sealed class NPCConversationGetGlimmerEvent : NPCConversationEvent
    {
        [DataField("text")]
        public readonly string? Text;
    }
}
// Hi! Solaris here, this is a test to see if the code will work as-is. Do not take this code seriously as this bullshit was probably written by Rane.
