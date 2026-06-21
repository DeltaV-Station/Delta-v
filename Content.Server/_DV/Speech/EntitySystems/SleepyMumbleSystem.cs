using Content.Shared.Speech;
using Content.Server._DV.Speech.Components;
using Content.Server._DV.Speech.SpeechTranslation;

using System.Text.RegularExpressions;

namespace Content.Server._DV.Speech.EntitySystems;

public sealed class SleepyMumbleSystem : EntitySystem
{
    private readonly SpeechTranslationSystem _st = new();

    public override void Initialize()
    {
        base.Initialize();

        _st.AddRule("huh", "whuh", probability: 0.85, position: MatchPosition.Nowhere);
        _st.AddRule(["going to", "gonna"], ["gunna", "gon'"], probability: 0.85, position: MatchPosition.Nowhere);
        _st.AddRule(["want to", "wanna"], ["wann'", "wunna"], probability: 0.80, position: MatchPosition.Nowhere);
        _st.AddRule(["have to", "gotta", "hafta"], ["hafta", "haft'"], probability: 0.75, position: MatchPosition.Nowhere);
        _st.AddRule(["kind of", "kinda"], ["kinda", "kind'"], probability: 0.70, position: MatchPosition.Nowhere);
        _st.AddRule(["sort of", "sorta"], ["sorta", "sort'"], probability: 0.70, position: MatchPosition.Nowhere);
        _st.AddRule("don't know", ["dunno", "d'no..."], probability: 0.85, position: MatchPosition.Nowhere);
        _st.AddRule(["what's that", "whats that"], ["wazzat...", "whassat"], probability: 0.75, position: MatchPosition.Nowhere);
        _st.AddRule(["what's going on", "whats going on"], ["wassgonn'", "whas' goin' on"], probability: 0.80, position: MatchPosition.Nowhere);

        _st.AddRule("ing", ["in'", "in"], probability: 0.85, position: MatchPosition.Suffix);
        _st.AddRule("ght", ["gh'", "t"], probability: 0.45, position: MatchPosition.Suffix);

        _st.AddRule(@"er\b", ["uh", "ur"], probability: 0.45, position: MatchPosition.Raw);
        _st.AddRule(@"or\b", "uh", probability: 0.35, position: MatchPosition.Raw);
        _st.AddRule(@"ar\b", "uh", probability: 0.30, position: MatchPosition.Raw);
        _st.AddRule(@"ly\b", ["leh", "l'"], probability: 0.40, position: MatchPosition.Raw);
        _st.AddRule(@"[ts]ion\b", ["sh'n", "shun"], probability: 0.65, position: MatchPosition.Raw);
        _st.AddRule(@"(?<=[aeiou])[nml][tdk]\b", ["n'", "m'", "nd"], probability: 0.50, position: MatchPosition.Raw);
        _st.AddRule(@"(?<=[aeiou])l(?=[^aeiou\s])", ["w", "'l"], probability: 0.25, position: MatchPosition.Raw);
        _st.AddRule(@"ck\b", ["g", "'k", "'"], probability: 0.30, position: MatchPosition.Raw);
        _st.AddRule(@"\bwh", ["w", "wuh"], probability: 0.50, position: MatchPosition.Raw);
        _st.AddRule("a", "aaa...", probability: 0.04, position: MatchPosition.Anywhere);
        _st.AddRule("e", "eee...", probability: 0.04, position: MatchPosition.Anywhere);
        _st.AddRule("i", "iii...", probability: 0.04, position: MatchPosition.Anywhere);
        _st.AddRule("o", "ooo...", probability: 0.04, position: MatchPosition.Anywhere);
        _st.AddRule("u", "uuu...", probability: 0.04, position: MatchPosition.Anywhere);

        _st.AddRule(@"\bhe\b", "'e", probability: 0.65, position: MatchPosition.Raw);
        _st.AddRule(@"\bhim\b", "'im", probability: 0.65, position: MatchPosition.Raw);
        _st.AddRule(@"\bher\b", "'er", probability: 0.65, position: MatchPosition.Raw);
        _st.AddRule(@"\bhis\b", "'is", probability: 0.50, position: MatchPosition.Raw);

        _st.AddRule("the", ["da", "d'", "th'", "thuh"], probability: 0.65, position: MatchPosition.Nowhere);
        _st.AddRule("and", ["an'", "n'", "'n"], probability: 0.70, position: MatchPosition.Nowhere);
        _st.AddRule("to", ["t'", "tuh", "ta"], probability: 0.55, position: MatchPosition.Nowhere);
        _st.AddRule("for", ["fr", "fer", "f'r"], probability: 0.55, position: MatchPosition.Nowhere);
        _st.AddRule("of", ["'f", "uh", "uv"], probability: 0.60, position: MatchPosition.Nowhere);
        _st.AddRule("are", ["r", "er", "'r"], probability: 0.55, position: MatchPosition.Nowhere);
        _st.AddRule("is", ["'z", "iz"], probability: 0.40, position: MatchPosition.Nowhere);
        _st.AddRule("it", ["'t", "i'"], probability: 0.35, position: MatchPosition.Nowhere);
        _st.AddRule("you", ["ya", "yuh", "y'"], probability: 0.65, position: MatchPosition.Nowhere);
        _st.AddRule("your", ["yer", "y'r", "yuh"], probability: 0.55, position: MatchPosition.Nowhere);
        _st.AddRule("can", ["c'n", "cn"], probability: 0.60, position: MatchPosition.Nowhere);
        _st.AddRule("just", ["jus'", "jus"], probability: 0.65, position: MatchPosition.Nowhere);
        _st.AddRule("that", ["tha'", "dat"], probability: 0.55, position: MatchPosition.Nowhere);
        _st.AddRule("with", ["wif", "wit", "w'"], probability: 0.55, position: MatchPosition.Nowhere);
        _st.AddRule("what", ["wha'", "wut", "wuh"], probability: 0.65, position: MatchPosition.Nowhere);
        _st.AddRule("not", ["nuh", "n't", "no'"], probability: 0.40, position: MatchPosition.Nowhere);
        _st.AddRule("but", ["bu'", "buh"], probability: 0.40, position: MatchPosition.Nowhere);
        _st.AddRule("was", ["wus", "wuz"], probability: 0.40, position: MatchPosition.Nowhere);
        _st.AddRule("my", ["m'", "mah", "meh"], probability: 0.45, position: MatchPosition.Nowhere);
        _st.AddRule("so", ["s'", "suh"], probability: 0.30, position: MatchPosition.Nowhere);
        _st.AddRule("do", ["d'", "duh"], probability: 0.30, position: MatchPosition.Nowhere);

        _st.AddRule("get", "ge'", probability: 0.50, position: MatchPosition.Nowhere);
        _st.AddRule("got", "go'", probability: 0.50, position: MatchPosition.Nowhere);
        _st.AddRule("getting", "gettin'", probability: 0.50, position: MatchPosition.Nowhere);
        _st.AddRule("probably", ["prolly", "prob'ly", "pro'ly"], probability: 0.85, position: MatchPosition.Nowhere);
        _st.AddRule("actually", ["actchully", "akshly"], probability: 0.75, position: MatchPosition.Nowhere);
        _st.AddRule("because", ["cuz", "coz", "'cause"], probability: 0.80, position: MatchPosition.Nowhere);
        _st.AddRule("okay", ["'kay", "mkay", "mhm..."], probability: 0.75, position: MatchPosition.Nowhere);
        _st.AddRule("something", ["summin'", "sumn'", "s'mthin'"], probability: 0.70, position: MatchPosition.Nowhere);
        _st.AddRule("everything", ["ev'rythin'", "evrythin'"], probability: 0.70, position: MatchPosition.Nowhere);
        _st.AddRule("nothing", ["nuthin'", "nothin'"], probability: 0.70, position: MatchPosition.Nowhere);
        _st.AddRule("anything", ["anythin'", "anythn'"], probability: 0.70, position: MatchPosition.Nowhere);
        _st.AddRule("remember", ["rember", "remb'r", "'member"], probability: 0.65, position: MatchPosition.Nowhere);
        _st.AddRule("about", ["'bout", "abou'"], probability: 0.55, position: MatchPosition.Nowhere);
        _st.AddRule("around", ["'round", "'roun'"], probability: 0.50, position: MatchPosition.Nowhere);
        _st.AddRule("again", ["'gain", "agen"], probability: 0.50, position: MatchPosition.Nowhere);
        _st.AddRule("before", ["b'fore", "'fore"], probability: 0.50, position: MatchPosition.Nowhere);
        _st.AddRule("together", ["t'gether", "t'geth'r"], probability: 0.55, position: MatchPosition.Nowhere);
        _st.AddRule("suppose", ["s'pose", "s'ppose"], probability: 0.60, position: MatchPosition.Nowhere);
        _st.AddRule(["already", "alright"], ["aight", "a'right"], probability: 0.60, position: MatchPosition.Nowhere);
        _st.AddRule("sorry", ["s'rry", "s'ry..."], probability: 0.60, position: MatchPosition.Nowhere);

        _st.AddRule(["hello", "hi", "hey"], ["'lo...", "m'h?", "wha...?", "hnh?"], probability: 0.75, position: MatchPosition.Nowhere);
        _st.AddRule(["yes", "yeah"], ["yeah...", "mhm", "yuh", "mm-hm..."], probability: 0.55, position: MatchPosition.Nowhere);
        _st.AddRule(["no", "nah"], ["nuh...", "nah...", "nnn", "mm-mm..."], probability: 0.55, position: MatchPosition.Nowhere);
        _st.AddRule(["thanks", "thank you"], ["thngks", "thns", "mhm"], probability: 0.50, position: MatchPosition.Nowhere);
        _st.AddRule("please", ["plz...", "plss"], probability: 0.50, position: MatchPosition.Nowhere);

        _st.AddRule(@"\.", ["...", ".."], probability: 0.45, position: MatchPosition.Raw);
        _st.AddRule(@"\?", ["...?", "..?"], probability: 0.50, position: MatchPosition.Raw);

        _st.AddRule(" ", new[] {
                " ...mmgh... ",   " ...mnh... ",   " ...hnnn... ",
                " ...rgh... ",    " ...nuh... ",   " ...mhm... ",
                " ...mrrnn... ",  " ...haah... ",  " ...ugh... ",
                " ...uhhh... ",   " ...zzz... ",   " ...m-mm... ",
                " ...wha... ",    " ...nnh... ",   " ...hnnng... ",
                " ...yaaawn... ", " ...mmmf... ",  " ...nggh... "
            },
            probability: 0.07, position: MatchPosition.Anywhere, applyOnce: true
        );

        SubscribeLocalEvent<SleepyMumbleComponent, AccentGetEvent>(OnAccent);
    }

    private static readonly Regex ExclamationRegex = new(@"!+", RegexOptions.Compiled);
    private static readonly Regex QuestionRegex = new(@"\?{2,}", RegexOptions.Compiled);

    private void OnAccent(EntityUid uid, SleepyMumbleComponent component, AccentGetEvent args)
    {
        var original = args.Message;

        var message = string.IsNullOrEmpty(original)
            ? original
            : char.ToUpperInvariant(original[0]) + original.Substring(1).ToLowerInvariant();
        message = ExclamationRegex.Replace(message, "...");
        message = QuestionRegex.Replace(message, "...?");

        args.Message = _st.Process(message);
    }
}
