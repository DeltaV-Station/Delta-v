using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class OwOAccentSystem : EntitySystem
    {
        private static IReadOnlyDictionary<string, string> SpecialWords = new Dictionary<string, string>();

        public override void Initialize()
        {
            SpecialWords = new Dictionary<string, string>(new KeyValuePair<string, string>[]
            {
                ReplacementSet("you", "yew"),
                ReplacementSet("yea", "yahuh"),
                ReplacementSet("yeah", "yahuh"),
                ReplacementSet("no", "nyo"),
                ReplacementSet("now", "nyow"),
                ReplacementSet("church", "choowch"),
                ReplacementSet("hop", "hoppy"),
                ReplacementSet("cap", "cappy"),
                ReplacementSet("what", "wot"),
                ReplacementSet("the", "da"),
                ReplacementSet("this", "dis"),
                ReplacementSet("that", "dat"),
                ReplacementSet("ow", "owies"),
                ReplacementSet("oww", "owwies"),
                ReplacementSet("ouch", "Owchies"),
                ReplacementSet("dead", "ded"),
                ReplacementSet("nuke", "nook"),
                ReplacementSet("nukie", "nookie"),
                ReplacementSet("sleep", "sleepy"),
                ReplacementSet("sec", "seccy"),
                ReplacementSet("secoff", "seccy"),
                ReplacementSet("fluff", "floof"),
                ReplacementSet("fluffy", "floofy"),
                ReplacementSet("mouse", "mowsie"),
                ReplacementSet("attention", "attenshun"),
                ReplacementSet("safe", "safes"),
                ReplacementSet("boss", "baws"),
                ReplacementSet("oh", "owh"),
                ReplacementSet("professional", "pwoffesshunal"),
                ReplacementSet("profession", "pwoffesshun"),
                ReplacementSet("confession", "confesshun"),
                ReplacementSet("food", "fewd"),
                ReplacementSet("move", "mewve"),
                ReplacementSet("moving", "mewving"),
                ReplacementSet("dog", "doggie"),
                ReplacementSet("however", "howeva"),
                ReplacementSet("hamster", "hammy"),
                ReplacementSet("hampster", "hammy"),
                ReplacementSet("antag", "baddie"),
                ReplacementSet("murder", "muwda"),
                ReplacementSet("rules", "roolz"),
                ReplacementSet("thanks", "thankies"),
                ReplacementSet("now", "nao"),
            });

            SubscribeLocalEvent<OwOAccentComponent, AccentGetEvent>(OnAccent);
        }

        public string Accentuate(string message)
        {
            foreach (var (word, repl) in SpecialWords)
            {
                message = message.Replace(word, repl);
            }

            return message.Replace("r", "w").Replace("R", "W")
                .Replace("l", "w").Replace("L", "W");
        }

        private void OnAccent(EntityUid uid, OwOAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }

        private static KeyValuePair<string, string> ReplacementSet(string original, string replacement)
        {
            if (IsAllUpper(original))
                replacement = replacement.ToUpper();
            if (char.IsUpper(original[0]))
                replacement = FirstCharToUpper(replacement);

            return new KeyValuePair<string, string>(original, replacement);
        }

        private static bool IsAllUpper(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsLetter(input[i]) && !char.IsUpper(input[i]))
                    return false;
            }
            return true;
        }

        private static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            return $"{char.ToUpper(input[0])}{input[1..]}";
        }
    }
}
