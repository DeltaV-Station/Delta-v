using Content.Server.Speech.Components;
using System.Text.RegularExpressions;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class OwOAccentSystem : EntitySystem
    {
        private static Dictionary<string, string> SpecialWords = new Dictionary<string, string>();

        public override void Initialize()
        {
            AddReplacementSet(SpecialWords, "yeah", "yahuh");
            AddReplacementSet(SpecialWords, "yea", "yahuh");
            AddReplacementSet(SpecialWords, "now", "nyow");
            AddReplacementSet(SpecialWords, "no", "nyo");
            AddReplacementSet(SpecialWords, "church", "choowch");
            AddReplacementSet(SpecialWords, "hop", "hoppy");
            AddReplacementSet(SpecialWords, "cap", "cappy");
            AddReplacementSet(SpecialWords, "what", "wot");
            AddReplacementSet(SpecialWords, "the", "da");
            AddReplacementSet(SpecialWords, "this", "dis");
            AddReplacementSet(SpecialWords, "that", "dat");
            AddReplacementSet(SpecialWords, "oww", "owwies");
            AddReplacementSet(SpecialWords, "ow", "owies");
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
            AddReplacementSet(SpecialWords, "stationary", "stashunawy");
            AddReplacementSet(SpecialWords, "god", "gawd");
            AddReplacementSet(SpecialWords, "cat", "kitty");
            AddReplacementSet(SpecialWords, "special", "speshul");
            AddReplacementSet(SpecialWords, "you", "yew");
            AddReplacementSet(SpecialWords, "super", "soopa");
            AddReplacementSet(SpecialWords, "supermatter", "soopamattew");
            AddReplacementSet(SpecialWords, "bartender", "bawtenda");
            AddReplacementSet(SpecialWords, "captain", "cappytan");
            AddReplacementSet(SpecialWords, "known", "knyown");
            AddReplacementSet(SpecialWords, "there", "dewe");
            AddReplacementSet(SpecialWords, "little", "widdle");
            AddReplacementSet(SpecialWords, "bite", "nom");
            AddReplacementSet(SpecialWords, "bye", "bai");
            AddReplacementSet(SpecialWords, "hell", "hecc");
            AddReplacementSet(SpecialWords, "hi", "hai");
            AddReplacementSet(SpecialWords, "love", "wuv");
            AddReplacementSet(SpecialWords, "good", "gewd");

            //special case that is more likely to have a different capitalisation
            SpecialWords.Add("HoP", "HoPpy");

            SubscribeLocalEvent<OwOAccentComponent, AccentGetEvent>(OnAccent);
        }

        public string Accentuate(string message)
        {
            foreach (var (word, repl) in SpecialWords)
            {
                message = Regex.Replace(message, $"\\b{word}\\b", repl);
            }

            return message.Replace("r", "w").Replace("R", "W")
                .Replace("l", "w").Replace("L", "W")
                .Replace("ck", "cc").Replace("Ck", "Cc")
                .Replace("cK", "cC").Replace("CK", "CC");
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
