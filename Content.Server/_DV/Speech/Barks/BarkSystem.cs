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

        if (profile.BarkVoice == null)
            return;

        if (_proto.HasIndex<BarkPrototype>(profile.BarkVoice) == false)
            return;

        var comp = EnsureComp<SpeechSynthesisComponent>(mob);
        comp.VoicePrototypeId = profile.BarkVoice;
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
            IsWhisper = args.ObfuscatedMessage != null
        };

        RaiseNetworkEvent(barkev, Filter.Pvs(uid));
    }
}
