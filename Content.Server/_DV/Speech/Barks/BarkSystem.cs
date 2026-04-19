using Content.Shared._DV.CCVars;
using Content.Shared._DV.Speech.Barks;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Speech;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Speech.Barks;
/// <summary>
/// This is the server side aspects for the Speech Barks feature.
/// Handles giving the system out on spawn and validating when a player speaks
/// </summary>
public sealed class BarkSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<SpeechSynthesisComponent, EntitySpokeEvent>(OnEntitySpoke);
    }
    /// <summary>
    /// Attaches and verifies the existence of Speech Bark components upon a character spawning
    /// </summary>
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        var profile = args.Profile;
        var mob = args.Mob;
        var voice = profile.BarkVoice;

        // If player hasn't selected a voice, assign a Species default
        if (voice == null || _proto.HasIndex<BarkPrototype>(voice) == false)
            voice = GetSpeciesDefaultVoice(profile.Species);

        // If still nothing, just default to Alto voice as a safety fallback
        voice ??= "Generic High";

        var comp = EnsureComp<SpeechSynthesisComponent>(mob);
        comp.VoicePrototypeId = voice;
    }

    private string? GetSpeciesDefaultVoice(string species)
    {

        // Go through the Speech Bark prototypes and compare species whitelisting, and assign the Species default Speech Barks voice
        foreach (var proto in _proto.EnumeratePrototypes<BarkPrototype>())
        {
            if (proto.SpeciesWhitelist != null && proto.SpeciesWhitelist.Contains(species))
                return proto.ID;
        }

        return null;
    }

    /// <summary>
    /// Verifies the Speech Bark component is set correctly then sends PlayBarkEvent to available clients
    /// </summary>
    private void OnEntitySpoke(EntityUid uid, SpeechSynthesisComponent comp, EntitySpokeEvent args)
    {
        if (comp.VoicePrototypeId == null)
            return;

        // Checks for server enabling of speech barks
        if (_cfg.GetCVar(DCCVars.BarksEnabled) == false)
            return;

        var barkev = new PlayBarkEvent
        {
            Speaker = GetNetEntity(uid),
            IsWhisper = args.ObfuscatedMessage != null,
            Message = args.Message
        };

        RaiseNetworkEvent(barkev, Filter.Pvs(uid));
    }
}
