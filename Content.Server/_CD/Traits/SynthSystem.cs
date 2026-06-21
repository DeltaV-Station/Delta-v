using Content.Server.Body.Systems;
using Content.Server.Database;
using Content.Shared.Body.Components;
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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SynthComponent, ComponentStartup>(OnStartup);
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

        // Begin DeltaV - Add SyntheticEmotes tag and merge synthetic emote sounds into species sounds
        _tag.AddTag(uid, SyntheticEmotesTag);

        if (TryComp<VocalComponent>(uid, out var vocal)
            && _proto.TryIndex(SyntheticEmoteSounds, out var synthSounds)
            && vocal.EmoteSounds is { } currentSoundsId
            && _proto.TryIndex(currentSoundsId, out var currentSounds))
        {
            foreach (var (emoteId, sound) in synthSounds.Sounds)
            {
                currentSounds.Sounds.TryAdd(emoteId, sound);
            }
            Dirty(uid, vocal);
        }
        // End DeltaV
    }
}