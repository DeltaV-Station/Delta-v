using System.Linq;
using System.Text;
using Content.Server._DV.Speech.Components;
using Content.Shared._DV.Speech.Prototypes;
using Content.Server.Speech;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Speech.EntitySystems;

public sealed class SyllableObfuscationAccentSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedGameTicker _ticker = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SyllableObfuscationAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, SyllableObfuscationAccentComponent component, AccentGetEvent args)
    {
        args.Message = ApplyReplacements(args.Message, component.Accent);
    }

    // stolen and slightly modified from EE: Content.Shared/_EinsteinEngines/Language/ObfuscationMethods.cs
    public string ApplyReplacements(string message, string accent)
    {
        if (!_proto.TryIndex<SyllableObfuscationAccentPrototype>(accent, out var prototype))
            return message;

        StringBuilder builder = new();

        const char eof = (char) 0; // Special character to mark the end of the message in the code below.

        var wordBeginIndex = 0;
        var hashCode = 0;

        for (var i = 0; i <= message.Length; i++)
        {
            var ch = i < message.Length ? char.ToLower(message[i]) : eof;
            var isWordEnd = char.IsWhiteSpace(ch) || IsPunctuation(ch) || ch == eof;

            // If this is a normal char, add it to the hash sum
            if (!isWordEnd)
                hashCode = hashCode * 31 + ch;

            // If a word ends before this character, construct a new word and append it to the new message.
            if (isWordEnd)
            {
                var wordLength = i - wordBeginIndex;
                if (wordLength > 0)
                {
                    var originalWord = message.Substring(wordBeginIndex, wordLength);
                    var isAllCaps = originalWord.All(c => !char.IsLetter(c) || char.IsUpper(c));

                    var newWord = new StringBuilder();
                    var newWordLength = Math.Clamp((originalWord.Length + 1) / 2,
                        prototype.MinSyllables,
                        prototype.MaxSyllables);

                    for (var j = 0; j < newWordLength; j++)
                    {
                        var index = PseudoRandomNumber(hashCode + j, 0, prototype.Replacement.Count - 1);
                        newWord.Append(prototype.Replacement[index]);
                    }
                    if (isAllCaps && wordLength > 1)
                        builder.Append(newWord.ToString().ToUpper());
                    else
                        builder.Append(newWord);
                }

                hashCode = 0;
                wordBeginIndex = i + 1;
            }

            // If this message concludes a word (i.e. is a whitespace or a punctuation mark), append it to the message
            if (isWordEnd && ch != eof)
                builder.Append(ch);
        }
        var result = builder.ToString();
        if (!string.IsNullOrEmpty(result)) // capitalize first character since capitals are lost from ic.punctuation
            result = char.ToUpperInvariant(result[0]) + result.Substring(1);
        return result;
    }

    private static bool IsPunctuation(char ch)
    {
        return ch is '.' or '!' or '?' or ',' or ':' or '-';
    }

    // EE function, doesn't use IRobustRandom for perf reasons, see Content.Shared/_EinsteinEngines/Language/Systems/SharedLanguageSystem.cs
    internal int PseudoRandomNumber(int seed, int min, int max)
    {
        seed = seed ^ (_ticker.RoundId * 127);
        var random = seed * 1103515245 + 12345;
        return min + Math.Abs(random) % (max - min + 1);
    }
}
