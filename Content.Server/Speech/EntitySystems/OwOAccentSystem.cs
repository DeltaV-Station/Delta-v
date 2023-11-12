using Content.Server.Speech.Components;
using System.Text.RegularExpressions;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class OwOAccentSystem : EntitySystem
    {
        // Dictionary to hold words to be replaced.
        private static Dictionary<string, string> SpecialWords = new Dictionary<string, string>();

        public override void Initialize()
        {
            AddAllReplacements();

            SubscribeLocalEvent<OwOAccentComponent, AccentGetEvent>(OnAccent);
        }

        public string Accentuate(string message)
        {
            // First replace any matched words by those registered in SpecialWords
            foreach (var (word, repl) in SpecialWords)
            {
                //Matches any whole words (so no spaces or punctuation) and replaces it if found
                message = Regex.Replace(message, $"\\b{word}\\b", repl);
            }

            // Replaces any occurrences of L's, R's, or CK's with a W, capitalized or not capitalized.
            return message.Replace("r", "w").Replace("R", "W")
                .Replace("l", "w").Replace("L", "W")
                .Replace("ck", "cc").Replace("Ck", "Cc")
                .Replace("cK", "cC").Replace("CK", "CC")
                .Replace("tt", "dd").Replace("Tt", "Dd")
                .Replace("tT", "dD").Replace("TT", "DD");
        }

        private void OnAccent(EntityUid uid, OwOAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }

        private static void AddReplacementSet(Dictionary<string, string> dictionary, string original, string replacement)
        {
            // Check if a key doesn't already exist. If it does not, add the new entry uncapitalized, First letter capitalized and ALL CAPS.
            if (!dictionary.ContainsKey(original.ToLower()))
            {
                dictionary.Add(original.ToLower(), replacement.ToLower());
                dictionary.Add(original.ToUpper(), replacement.ToUpper());
                dictionary.Add(FirstCharToUpper(original.ToLower()), FirstCharToUpper(replacement.ToLower()));
            }
        }

        private static void AddSpecialReplacementCase(Dictionary<string, string> dictionary, string original, string replacement)
        {
            // Check if a key doesn't already exist. If it does not, add the new entry directly as input in the method
            if (!dictionary.ContainsKey(original))
            {
                dictionary.Add(original, replacement);
            }
        }

        private static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // Capitalize the char at index [0], and then append the remainder of the original string.
            return $"{char.ToUpper(input[0])}{input[1..]}";
        }

        private void AddAllReplacements()
        {
            // Register words to be replaced to the SpecialWords dictionary
            // This automatically includes Capitalized and FULLCAPS variants.
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
            AddReplacementSet(SpecialWords, "super", "supa");
            AddReplacementSet(SpecialWords, "supermatter", "supamattew");
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
            AddReplacementSet(SpecialWords, "with", "wiv");
            AddReplacementSet(SpecialWords, "box", "bawx");
            AddReplacementSet(SpecialWords, "shuttle", "shuddle");
            AddReplacementSet(SpecialWords, "instead", "steads");
            AddReplacementSet(SpecialWords, "got", "gots");
            AddReplacementSet(SpecialWords, "need", "needs");
            AddReplacementSet(SpecialWords, "doctor", "docta");
            AddReplacementSet(SpecialWords, "say", "says");
            AddReplacementSet(SpecialWords, "ok", "okie dokies");
            AddReplacementSet(SpecialWords, "same", "sames");
            AddReplacementSet(SpecialWords, "see", "sees");
            AddReplacementSet(SpecialWords, "after", "afta");
            AddReplacementSet(SpecialWords, "better", "bedda");
            AddReplacementSet(SpecialWords, "wow", "wowie");
            AddReplacementSet(SpecialWords, "woah", "wowie");
            AddReplacementSet(SpecialWords, "bump", "boomp");
            AddReplacementSet(SpecialWords, "zombie", "zoombie");
            AddReplacementSet(SpecialWords, "nanotrasen", "nyanotwasen");
            AddReplacementSet(SpecialWords, "bomb", "bomba");
            AddReplacementSet(SpecialWords, "lookout", "wookouts");
            AddReplacementSet(SpecialWords, "out", "outs");
            AddReplacementSet(SpecialWords, "nice", "nyice");
            AddReplacementSet(SpecialWords, "music", "moosic");
            AddReplacementSet(SpecialWords, "help", "helps");
            AddReplacementSet(SpecialWords, "bird", "bwerd");
            AddReplacementSet(SpecialWords, "just", "joost");
            AddReplacementSet(SpecialWords, "so", "sos");
            AddReplacementSet(SpecialWords, "sabotaging", "sabootazing");
            AddReplacementSet(SpecialWords, "sabotage", "sabootaze");
            AddReplacementSet(SpecialWords, "me", "mes");
            AddReplacementSet(SpecialWords, "person", "persoon");
            AddReplacementSet(SpecialWords, "up", "ups");
            AddReplacementSet(SpecialWords, "cool", "kewl");

            //special case that is more likely to have a different capitalisation not already included in the above list
            //These entries are added to the dictionary as-is, for special capitalization cases only.
            AddSpecialReplacementCase(SpecialWords, "HoP", "HoPpy");
        }
    }
}
