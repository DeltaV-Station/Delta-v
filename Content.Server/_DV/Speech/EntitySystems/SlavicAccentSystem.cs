using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class SlavicAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    // Sound replacement regexes
    private static readonly Regex ThToZVowelRegex = new(@"\bTh(?=[aeiou])", RegexOptions.Compiled);
    private static readonly Regex ThToZWordsRegex = new(@"Th(?=at|is|ese|ose|ey|em|an)", RegexOptions.Compiled);
    private static readonly Regex AllCapsThToZVowelRegex = new(@"\bTH(?=[AEIOU])", RegexOptions.Compiled);
    private static readonly Regex AllCapsThToZWordsRegex = new(@"TH(?=AT|IS|ESE|OSE|EY|EM|AN)", RegexOptions.Compiled);
    private static readonly Regex LowercaseThToZVowelRegex = new(@"\bth(?=[aeiou])", RegexOptions.Compiled);
    private static readonly Regex LowercaseThToZWordsRegex = new(@"th(?=at|is|ese|ose|ey|em|an)", RegexOptions.Compiled);
    private static readonly Regex CToKCapitalRegex = new(@"\bC", RegexOptions.Compiled);
    private static readonly Regex CToKLowercaseRegex = new(@"\bc", RegexOptions.Compiled);
    private static readonly Regex WToVCapitalRegex = new(@"\bW", RegexOptions.Compiled);
    private static readonly Regex WToVLowercaseRegex = new(@"\bw", RegexOptions.Compiled);
    private static readonly Regex DentalTInVowelsRegex = new(@"(?<=[aeiouAEIOU])t(?=[aeiouAEIOU])", RegexOptions.Compiled);
    private static readonly Regex EeRegex = new(@"ee", RegexOptions.Compiled);

    // Grammar replacement regexes
    private static readonly Regex TheLowercaseRegex = new(@"\bthe\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ALowercaseRegex = new(@"\ba\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex AnLowercaseRegex = new(@"\ban\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex IsLowercaseRegex = new(@"\bis\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex AreLowercaseRegex = new(@"\bare\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex IAmLowercaseRegex = new(@"\bI am\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@" +", RegexOptions.Compiled);

    private static readonly Regex TovarischRegex = new(@"\btovarisch\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public override void Initialize()
    {
        SubscribeLocalEvent<SlavicAccentComponent, AccentGetEvent>(OnAccent);
    }

    // Applies Slavic accent to a message
    public string Accentuate(string message, SlavicAccentComponent component)
    {
        var accentedMessage = _replacement.ApplyReplacements(message, "slavic");
        accentedMessage = ApplyKomradeReplacement(accentedMessage, component);
        accentedMessage = ApplyGrammarRules(accentedMessage, component);
        accentedMessage = ApplySoundReplacements(accentedMessage);
        return accentedMessage;
    }

    // Randomly replaces 'tovarisch' with 'Komrade' while preserving capitalization.
    // TODO: The ReplacementAccentSystem REALLY should have random replacements built-in.
    private string ApplyKomradeReplacement(string message, SlavicAccentComponent component)
    {
        return TovarischRegex.Replace(message, match =>
        {
            if (!_random.Prob(component.KomradeReplacementChance))
                return match.Value;
            var original = match.Value;
            if (IsAllUpperCase(original))
                return "KOMRADE";
            if (IsCapitalized(original))
                return "Komrade";
            return "komrade";
        });
    }

    private static bool IsAllUpperCase(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        foreach (var c in text)
            if (char.IsLetter(c) && !char.IsUpper(c)) return false;
        return true;
    }

    private static bool IsCapitalized(string text)
    {
        if (string.IsNullOrEmpty(text) || !char.IsLetter(text[0])) return false;
        if (!char.IsUpper(text[0])) return false;
        for (int i = 1; i < text.Length; i++)
            if (char.IsLetter(text[i]) && !char.IsLower(text[i])) return false;
        return true;
    }

    // Applies sound-level replacements to simulate Slavic accent phonetics.
    private string ApplySoundReplacements(string message)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        var result = message;

        // Apply TH replacements (grouped by case)
        result = ThToZVowelRegex.Replace(result, "Z");
        result = ThToZWordsRegex.Replace(result, "Z");
        result = AllCapsThToZVowelRegex.Replace(result, "Z");
        result = AllCapsThToZWordsRegex.Replace(result, "Z");
        result = LowercaseThToZVowelRegex.Replace(result, "z");
        result = LowercaseThToZWordsRegex.Replace(result, "z");

        // Apply other consonant replacements
        result = CToKCapitalRegex.Replace(result, "K");
        result = CToKLowercaseRegex.Replace(result, "k");
        result = WToVCapitalRegex.Replace(result, "V");
        result = WToVLowercaseRegex.Replace(result, "v");

        // Apply vowel and other sound changes
        result = DentalTInVowelsRegex.Replace(result, "th");
        result = EeRegex.Replace(result, "i");

        // Restore capitalization
        if (result.Length > 0 && message.Length > 0 &&
            char.IsLetter(message[0]) && char.IsLower(result[0]) && char.IsUpper(message[0]))
        {
            result = char.ToUpper(result[0]) + result.Substring(1);
        }

        return result;
    }

    // Applies grammar rules typical of Slavic-accented English, such as article and verb removal.
    private string ApplyGrammarRules(string message, SlavicAccentComponent component)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        var wasFirstLetterCapitalized = char.IsUpper(message[0]);

        // Early exit if its not long enough
        var wordCount = message.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        if (wordCount <= 3)
        {
            return message;
        }

        // Instead of checking per word we're just gonna check for the whole message.
        if (!_random.Prob(component.ArticleRemovalChance))
        {
            return message;
        }

        var result = message;

        // If a message starts with any of these we remove em if ArticalRemovalChance passes. This is more optimized than doing a regex check.
        if (result.StartsWith("The ", StringComparison.Ordinal))
            result = result.Substring(4);
        else if (result.StartsWith("THE ", StringComparison.Ordinal))
            result = result.Substring(4);
        else if (result.StartsWith("A ", StringComparison.Ordinal))
            result = result.Substring(2);
        else if (result.StartsWith("An ", StringComparison.Ordinal))
            result = result.Substring(3);
        else
        {
            // Apply regex replacements for articles elsewhere in the message
            result = TheLowercaseRegex.Replace(result, "");
            result = ALowercaseRegex.Replace(result, "");
            result = AnLowercaseRegex.Replace(result, "");
        }

        // Remove verbs
        result = IsLowercaseRegex.Replace(result, "");
        result = AreLowercaseRegex.Replace(result, "");

        // Simplify "I am" to "I"
        result = IAmLowercaseRegex.Replace(result, "I");

        // Clean up whitespace
        result = WhitespaceRegex.Replace(result.Trim(), " ");

        // Restore capitalization
        if (wasFirstLetterCapitalized && !string.IsNullOrEmpty(result) && char.IsLetter(result[0]) && char.IsLower(result[0]))
        {
            result = char.ToUpper(result[0]) + result.Substring(1);
        }

        return result;
    }

    private void OnAccent(EntityUid uid, SlavicAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
