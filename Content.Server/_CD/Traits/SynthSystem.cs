using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Chat;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Chat.TypingIndicator;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.Traits;

public sealed class SynthSystem : EntitySystem
{
    // Begin DeltaV - make strings static readonly
    private static readonly ProtoId<TypingIndicatorPrototype> RobotTypingIndicator = "robot";
    private static readonly ProtoId<ReagentPrototype> SynthBloodReagent = "SynthBlood";
    private static readonly ProtoId<EmoteSoundsPrototype> SyntheticEmoteSounds = "SyntheticEmoteSounds";
    private static readonly ProtoId<EmotePrototype>[] SiliconEmotes = ["Beep", "Chime", "Buzz", "Buzz-Two", "Ping"];
    // End DeltaV

    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SynthComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SynthComponent, EmoteEvent>(OnEmote);
    }

    private void OnStartup(EntityUid uid, SynthComponent component, ComponentStartup args)
    {
        if (TryComp<TypingIndicatorComponent>(uid, out var indicator))
        {
            indicator.TypingIndicatorPrototype = RobotTypingIndicator; // DeltaV - make strings static readonly
            Dirty(uid, indicator);
        }

        if (TryComp<BloodstreamComponent>(uid, out var bloodstream))
        {
            // Give them synth blood. Ion storm notif is handled in that system
            _bloodstream.ChangeBloodReagents((uid, bloodstream), new([new(SynthBloodReagent, bloodstream.BloodReferenceSolution.Volume)]));
        }

        // Add the silicon emotes to the allow list so we can ignore the emote whitelist/blacklist. This just allows the emote event to
        // be sent but it does not associate a sound with the emote. We'll intercept that event in OnEmote and play the sound manually there
        // since we cannot add just add sounds on the fly to the emote sounds of a species (everyone will have a different sound).
        if (HasComp<VocalComponent>(uid) && TryComp<SpeechComponent>(uid, out var speech))
        {
            speech.AllowedEmotes.AddRange(SiliconEmotes);
            Dirty(uid, speech);
        }
    }

    private void OnEmote(EntityUid uid, SynthComponent component, ref EmoteEvent args)
    {
        if (args.Handled)
            return;

        // If we make it this far, its an allowed emote, so just resolve the sound from
        // the SyntheticEmoteSounds prototype and play it.
        if (_proto.Resolve(SyntheticEmoteSounds, out var emoteSound))
            args.Handled = _chat.TryPlayEmoteSound(uid, emoteSound, args.Emote);
    }
}