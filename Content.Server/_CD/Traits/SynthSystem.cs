using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Chat;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Chat.TypingIndicator;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Speech.Components;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.Traits;

public sealed class SynthSystem : EntitySystem
{
    // Begin DeltaV - make strings static readonly
    private static readonly ProtoId<TypingIndicatorPrototype> RobotTypingIndicator = "robot";
    private static readonly ProtoId<ReagentPrototype> SynthBloodReagent = "SynthBlood";
    private static readonly ProtoId<TagPrototype> SyntheticEmotesTag = "SyntheticEmotes";
    private static readonly ProtoId<EmoteSoundsPrototype> SyntheticEmoteSounds = "SyntheticEmoteSounds";
    // End DeltaV

    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    private EmoteSoundsPrototype? _syntheticEmoteSounds; // delta-v - proper synthe emote implementation

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SynthComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SynthComponent, EmoteEvent>(OnEmote); // delta-v - proper synthe emote implementation
    }

    private void OnStartup(EntityUid uid, SynthComponent component, ComponentStartup args)
    {
        if (TryComp<TypingIndicatorComponent>(uid, out var indicator))
        {
            indicator.TypingIndicatorPrototype = RobotTypingIndicator; // DeltaV - make strings static readonly
            Dirty(uid, indicator);
        }

        // Begin DeltaV - Change blood amount according to original BloodstreamCompoent.ReferenceSolution volume
        if (TryComp<BloodstreamComponent>(uid, out var bloodstream))
        {
            // Give them synth blood. Ion storm notif is handled in that system
            _bloodstream.ChangeBloodReagents((uid, bloodstream), new([new(SynthBloodReagent, bloodstream.BloodReferenceSolution.Volume)]));
        }
        // End DeltaV

        _tag.AddTag(uid, SyntheticEmotesTag); // delta-v - proper synthe emote implementation
    }

    // Start DeltaV - proper synthe emote implementation
    private void OnEmote(EntityUid uid, SynthComponent component, ref EmoteEvent args)
    {
        if (args.Handled)
            return;

        _syntheticEmoteSounds ??= _proto.Index(SyntheticEmoteSounds);

        args.Handled = _chat.TryPlayEmoteSound(uid, _syntheticEmoteSounds, args.Emote);
    }
    // End DeltaV
}