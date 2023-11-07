using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class OwOAccentSystem : EntitySystem
    {
        private static Dictionary<string, string> SpecialWords = new Dictionary<string, string>();

        public override void Initialize()
        {
            AddReplacementSet(SpecialWords, "you", "yew");
            AddReplacementSet(SpecialWords, "yea", "yahuh");
            AddReplacementSet(SpecialWords, "yeah", "yahuh");
            AddReplacementSet(SpecialWords, "no", "nyo");
            AddReplacementSet(SpecialWords, "now", "nyow");
            AddReplacementSet(SpecialWords, "church", "choowch");
            AddReplacementSet(SpecialWords, "hop", "hoppy");
            AddReplacementSet(SpecialWords, "cap", "cappy");
            AddReplacementSet(SpecialWords, "what", "wot");
            AddReplacementSet(SpecialWords, "the", "da");
            AddReplacementSet(SpecialWords, "this", "dis");
            AddReplacementSet(SpecialWords, "that", "dat");
            AddReplacementSet(SpecialWords, "ow", "owies");
            AddReplacementSet(SpecialWords, "oww", "owwies");
            AddReplacementSet(SpecialWords, "ouch", "Owchies");
            AddReplacementSet(SpecialWords, "dead", "ded");
            AddReplacementSet(SpecialWords, "nuke", "nook");
            AddReplacementSet(SpecialWords, "nukie", "nookie");
            AddReplacementSet(SpecialWords, "sleep", "sweepy");
            AddReplacementSet(SpecialWords, "sec", "seccy");
            AddReplacementSet(SpecialWords, "secoff", "seccy");
            AddReplacementSet(SpecialWords, "fluff", "fwoof");
            AddReplacementSet(SpecialWords, "fluffy", "fwoofy");
            AddReplacementSet(SpecialWords, "mouse", "mowsie");
            AddReplacementSet(SpecialWords, "attention", "attenshun");
            AddReplacementSet(SpecialWords, "safe", "safes");
            AddReplacementSet(SpecialWords, "boss", "baws");
            AddReplacementSet(SpecialWords, "oh", "owh");
            AddReplacementSet(SpecialWords, "professional", "pwoffesshunaw");
            AddReplacementSet(SpecialWords, "profession", "pwoffesshun");
            AddReplacementSet(SpecialWords, "confession", "confesshun");
            AddReplacementSet(SpecialWords, "food", "fewd");
            AddReplacementSet(SpecialWords, "move", "mewve");
            AddReplacementSet(SpecialWords, "moving", "mewving");
            AddReplacementSet(SpecialWords, "dog", "doggie");
            AddReplacementSet(SpecialWords, "however", "howeva");
            AddReplacementSet(SpecialWords, "hamster", "hammy");
            AddReplacementSet(SpecialWords, "hampster", "hammy");
            AddReplacementSet(SpecialWords, "antag", "baddie");
            AddReplacementSet(SpecialWords, "murder", "muwda");
            AddReplacementSet(SpecialWords, "rules", "woolez");
            AddReplacementSet(SpecialWords, "thanks", "thankies");
            AddReplacementSet(SpecialWords, "station", "stashun");
            AddReplacementSet(SpecialWords, "god", "gawd");
            AddReplacementSet(SpecialWords, "cat", "kitty");
            AddReplacementSet(SpecialWords, "special", "speshul");

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
        private static void AddReplacementSet(Dictionary<string, string> dictionary, string original, string replacement)
        {
            dictionary.Add(original, replacement);
            dictionary.Add(original.ToUpper(), replacement.ToUpper());
            dictionary.Add(FirstCharToUpper(original), FirstCharToUpper(replacement));
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
