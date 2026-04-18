using Content.Shared._DV.CCVars;
using Content.Shared._DV.Speech.Barks;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Audio;
using System.Linq;

namespace Content.Client._DV.Speech.Barks;

/// <summary>
/// The client side Speech Barks system, to recieve event from server and play audio blips based on speech
/// </summary>
public sealed class BarkSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    /// <summary>
    /// Tracks an active Speech Bark playing blips
    /// </summary>
    private sealed class ActiveBark
    {
        public EntityUid Entity;
        public BarkPrototype Prototype = default!;
        public string Message = default!;
        public bool IsWhisper;
        public bool IsExclaim;
        public int CurrentChar;
        public int TotalSounds;
        public float Timer;
        public float Interval;
    }

    private readonly List<ActiveBark> _activeBarks = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<PlayBarkEvent>(OnPlayBark);
    }

    /// <summary>
    /// Counts digraphs/syllables within the text
    /// </summary>
    private int DigraphCount(string text)
    {
        int count = 0;
        bool lastWasVowel = false;

        foreach (var ch in text.ToLower())
        {
            var isVowel = "aeiouy".Contains(ch);
            if (isVowel && (lastWasVowel == false))
                count++;
            lastWasVowel = isVowel;
        }

        return Math.Max(1, count);
    }

    private void OnPlayBark(PlayBarkEvent args)
    {
        var entity = GetEntity(args.Speaker);

        if (TryComp<SpeechSynthesisComponent>(entity, out var synth) == false)
            return;

        if (synth.VoicePrototypeId == null)
            return;

        if (_proto.TryIndex<BarkPrototype>(synth.VoicePrototypeId, out var prototype) == false)
            return;

        // Caps the message length for the audio calculations
        var message = args.Message ?? "";
        if (message.Length > 50)
            message = message[..50];

        // Calculates the timing. interval is spacing, digraphs is the total message count, totalSounds is min digraphs or max 25
        var interval = 1f;
        var digraphs = DigraphCount(message);
        var totalSounds = Math.Min(digraphs, 15);
        var isExclaim = message.EndsWith("!!");

        _activeBarks.Add(new ActiveBark
        {
            Entity = entity,
            Prototype = prototype,
            Message = message,
            IsWhisper = args.IsWhisper,
            IsExclaim = isExclaim,
            CurrentChar = 0,
            TotalSounds = totalSounds,
            Timer = 0f,
            Interval = interval,
        });
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var volume = _cfg.GetCVar(DCCVars.BarksVolume);

        // Runs the list backward
        for (var i = _activeBarks.Count -1; i >= 0; i--)
        {
            var bark = _activeBarks[i];

            bark.Timer -= frameTime;

            if (bark.Timer > 0f)
                continue;

            // Resets the timer for the next blip
            bark.Timer += bark.Interval;

            // Find the next none space character
            while (bark.CurrentChar < bark.Message.Length &&
                   bark.Message[bark.CurrentChar] is ' ' or '-')
            {
                bark.CurrentChar++;
            }

            // Finishes once all the sounds are played
            if (bark.TotalSounds <= 0 || bark.CurrentChar >= bark.Message.Length)
            {
                _activeBarks.RemoveAt(i);
                continue;
            }

            // Calculates the pitch
            float pitch;
            var proto = bark.Prototype;
            var ch = bark.Message[bark.CurrentChar];

            if (proto.Predictable)
            {
                // The same digraphs get the same pitch, every time
                var hash = ch.GetHashCode();
                pitch = proto.MinPitch + (Math.Abs(hash) % 100) / 100f * (proto.MaxPitch - proto.MinPitch);
            }
            else
            {
                pitch = _random.NextFloat(proto.MinPitch, proto.MaxPitch);

            }

            // Calculates the volume
            var volu = proto.Volume + (volume / 3f);
            if (bark.IsWhisper)
                volu -= 5f;

            if (bark.IsExclaim)
            {
                volu += 15f;
                pitch += 0.35f;
            }
             
            // Playing the sound
            _audio.PlayPvs(
                proto.Sounds,
                bark.Entity,
                AudioParams.Default.WithPitchScale(pitch).WithVolume(volu));

            bark.CurrentChar++;
            bark.TotalSounds--;
        }
    }
}

