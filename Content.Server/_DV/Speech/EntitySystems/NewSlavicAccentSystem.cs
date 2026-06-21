using System.Text.RegularExpressions;
using Content.Shared.Speech;
using Content.Server._DV.Speech.Components;
using Content.Server._DV.Speech.SpeechTranslation;

namespace Content.Server._DV.Speech.EntitySystems;

public sealed class NewSlavicAccentSystem : EntitySystem
{
    private readonly SpeechTranslationSystem _st = new();
    private readonly SpeechTranslationSystem _grammarDrop = new();
    private readonly SpeechTranslationSystem _phonetics = new();

    private const string ProtectStart = "\uE000";
    private const string ProtectEnd = "\uE001";
    private const string ProtectPlaceholder = "\uE002";
    private static readonly Regex ProtectedSpanRegex = new(ProtectStart + "(.*?)" + ProtectEnd, RegexOptions.Compiled);
    private static readonly Regex PlaceholderRegex = new(ProtectPlaceholder + @"(\d+)" + ProtectPlaceholder, RegexOptions.Compiled);

    private static string Protect(string word) => ProtectStart + word + ProtectEnd;

    private static string[] Protect(params string[] words)
    {
        var result = new string[words.Length];
        for (var i = 0; i < words.Length; i++)
            result[i] = Protect(words[i]);
        return result;
    }

    private const int MinWordCountForGrammarDrop = 3;

    public override void Initialize()
    {
        base.Initialize();

        _st.AddRule(
            ["friend", "pal", "buddy", "bro", "brother", "mate", "dude"],
            Protect("tovarisch", "comrade"),
            probability: 0.50
        );
        _st.AddRule(
            ["sir", "mister", "boss", "man", "guy"],
            Protect("droog"),
            probability: 0.45
        );
        _st.AddRule(
            ["girl", "lady", "woman", "ma'am", "maam", "miss"],
            Protect("devushka"),
            probability: 0.45
        );
        _st.AddRule(["doctor", "physician", "medic", "paramedic", "medical doctor"], Protect("doktor"), probability: 0.50);
        _st.AddRule(["scientist", "researcher"], Protect("uchyony"), probability: 0.50);
        _st.AddRule(["engineer", "mechanic", "tech"], Protect("inzhener"), probability: 0.50);
        _st.AddRule(["captain", "commander", "officer"], Protect("kapitan"), probability: 0.50);
        _st.AddRule(["security", "guard", "cop", "warden"], Protect("militsiya"), probability: 0.50);
        _st.AddRule(["janitor", "cleaner", "custodian"], Protect("dvornik"), probability: 0.45);
        _st.AddRule(["comrade", "colleague", "coworker"], Protect("tovarisch"), probability: 0.60);
        _st.AddRule(
            ["syndi", "syndie", "syndicate", "nukie", "nuke op", "nuclear operative", "traitor", "saboteur"],
            Protect("kapitalist shpion", "enemy of people"),
            probability: 0.50
        );

        _st.AddRule(["hello", "hi", "hey", "greetings"], Protect("privet"));
        _st.AddRule(["goodbye", "bye", "farewell", "see you", "later"], Protect("dosvedanya"));

        _st.AddRule(["yes", "yeah", "yep", "yea"], Protect("da"));
        _st.AddRule(["no", "nope", "nah"], Protect("nyet"));
        _st.AddRule(["damn", "dammit", "crap", "shoot", "ugh", "argh"], Protect("blyat"), probability: 0.55);
        _st.AddRule(["what the hell", "what the fuck", "what the"], Protect("blyat"), probability: 0.50);
        _st.AddRule(["idiot", "fool", "moron", "imbecile", "dumbass", "stupid"], Protect("durak", "mudak"), probability: 0.55);
        _st.AddRule(["nonsense", "bullshit", "lies", "rubbish"], Protect("blyat"), probability: 0.50);
        _st.AddRule(["shut up", "quiet", "silence"], Protect("zatknis"), probability: 0.50);

        _st.AddRule(@"\bI am\b", Protect("I"), probability: 0.35, position: MatchPosition.Raw);

        _st.AddRule("think", Protect("am zinking"), probability: 0.40);
        _st.AddRule("believe", Protect("am believing"), probability: 0.40);

        _st.AddRule("it is", Protect("ees"), probability: 0.50);
        _st.AddRule("there is", Protect("dere ees"), probability: 0.45);
        _st.AddRule("there are", Protect("dere are"), probability: 0.45);

        _grammarDrop.AddRule(@"\bthe\b", "", probability: 0.30, position: MatchPosition.Raw);
        _grammarDrop.AddRule(@"\ba\b", "", probability: 0.25, position: MatchPosition.Raw);
        _grammarDrop.AddRule(@"\ban\b", "", probability: 0.25, position: MatchPosition.Raw);

        _grammarDrop.AddRule(@"\bis\b", "", probability: 0.20, position: MatchPosition.Raw);
        _grammarDrop.AddRule(@"\bare\b", "", probability: 0.20, position: MatchPosition.Raw);

        _phonetics.AddRule(@"\bth(e|is|em|ey|ese|ose|at|ere|en|ough|ink|ought|rough|ree|row)\b", "z$1", probability: 0.80, position: MatchPosition.Raw);
        _phonetics.AddRule(@"th", "z", probability: 0.75, position: MatchPosition.Raw);

        _phonetics.AddRule(@"ee", "i", probability: 0.35, position: MatchPosition.Raw);

        _phonetics.AddRule(@"\bw", "v", probability: 0.80, position: MatchPosition.Raw);
        _phonetics.AddRule(@"\bwh", "v", probability: 0.70, position: MatchPosition.Raw);
        _phonetics.AddRule(@"(?<=[aeiou])v(?=[aeiou])", "w", probability: 0.25, position: MatchPosition.Raw);

        _phonetics.AddRule(@"\bis\b", "ees", probability: 0.65, position: MatchPosition.Raw);
        _phonetics.AddRule(@"\bit\b", "eet", probability: 0.55, position: MatchPosition.Raw);
        _phonetics.AddRule(@"\bin\b", "een", probability: 0.40, position: MatchPosition.Raw);

        _phonetics.AddRule(@"\bh(?=[aeiou])", "kh", probability: 0.40, position: MatchPosition.Raw);
        _phonetics.AddRule(@"\br(?=[aeiou])", "rr", probability: 0.45, position: MatchPosition.Raw);

        _phonetics.AddRule(@"d\b", "t", probability: 0.40, position: MatchPosition.Raw);
        _phonetics.AddRule(@"b\b", "p", probability: 0.35, position: MatchPosition.Raw);
        _phonetics.AddRule(@"g\b", "k", probability: 0.35, position: MatchPosition.Raw);
        _phonetics.AddRule(@"z\b", "s", probability: 0.35, position: MatchPosition.Raw);
        _phonetics.AddRule(@"v\b", "f", probability: 0.35, position: MatchPosition.Raw);

        _phonetics.AddRule(@"ing\b", "ink", probability: 0.50, position: MatchPosition.Raw);
        _phonetics.AddRule(@"tion\b", "tsion", probability: 0.55, position: MatchPosition.Raw);
        _phonetics.AddRule(@"\bj(?=[aeiou])", "y", probability: 0.60, position: MatchPosition.Raw);
        _phonetics.AddRule(@"oo", "u", probability: 0.35, position: MatchPosition.Raw);
        _phonetics.AddRule(@"ou", "u", probability: 0.30, position: MatchPosition.Raw);
        _phonetics.AddRule(@"ck", "k", probability: 0.45, position: MatchPosition.Raw);
        _phonetics.AddRule(@"(?<=[^aeiou])ed\b", "et", probability: 0.45, position: MatchPosition.Raw);

        SubscribeLocalEvent<NewSlavicAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, NewSlavicAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        var processed = _st.Process(message);

        var protectedWords = new List<string>();
        processed = ProtectedSpanRegex.Replace(processed, match =>
        {
            protectedWords.Add(match.Groups[1].Value);
            return ProtectPlaceholder + (protectedWords.Count - 1) + ProtectPlaceholder;
        });

        var wordCount = message.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        if (wordCount > MinWordCountForGrammarDrop)
            processed = _grammarDrop.Process(processed);

        processed = _phonetics.Process(processed);

        processed = PlaceholderRegex.Replace(processed, match => protectedWords[int.Parse(match.Groups[1].Value)]);

        args.Message = processed;
    }
}