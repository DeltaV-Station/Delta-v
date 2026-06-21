// NOT for a language system, but rather used for accents instead.
// Honestly, a lot better for me to do this instead of tinkering around fluent files...
// (i dont think accents would be localized in non-latin speech?? (ru/sp))

using System.Linq;
using System.Text.RegularExpressions;

namespace Content.Server._DV.Speech.SpeechTranslation
{
    public enum CapitalizationMode
    {
        PerLetter,
        FirstLetter,
        AllUpper,
        AllLower,
        Preserve
    }

    public enum MatchPosition
    {
        Anywhere,
        Nowhere,
        Prefix,
        Suffix,
        Raw
    }

    public static class CommonPatterns
    {
        public const string AnyVowel = "[aeiou]";
        public const string AnyConsonant = "[bcdfghjklmnpqrstvwxyz]";
        public const string AnyLetter = @"\p{L}";
        public const string AnyDigit = @"\d";
        public const string Whitespace = @"\s+";
        public const string WholeWord = @"\b\w+\b";
        public const string CapitalizedWord = @"\b[A-Z]\w*\b";
    }

    public sealed class ReplacementRule
    {
        public IReadOnlyList<string> Patterns { get; }
        public IReadOnlyList<string> Replacements { get; }
        public CapitalizationMode CapMode { get; }
        public double Probability { get; }
        public bool ApplyOnce { get; }
        public MatchPosition Position { get; }
        public bool CaseSensitive { get; }
        internal Regex CompiledPattern { get; }

        public ReplacementRule(
            IEnumerable<string> patterns,
            IEnumerable<string> replacements,
            CapitalizationMode capMode = CapitalizationMode.PerLetter,
            double probability = 1.0,
            bool applyOnce = false,
            MatchPosition position = MatchPosition.Nowhere,
            bool caseSensitive = false)
        {
            var patternList = (patterns ?? throw new ArgumentNullException(nameof(patterns))).ToList();
            var replacementList = (replacements ?? throw new ArgumentNullException(nameof(replacements))).ToList();

            if (patternList.Count == 0)
                throw new ArgumentException("At least one pattern is required.", nameof(patterns));
            if (replacementList.Count == 0)
                throw new ArgumentException("At least one replacement is required.", nameof(replacements));

            Patterns = patternList.AsReadOnly();
            Replacements = replacementList.AsReadOnly();
            CapMode = capMode;
            Probability = Math.Clamp(probability, 0.0, 1.0);
            ApplyOnce = applyOnce;
            Position = position;
            CaseSensitive = caseSensitive;

            CompiledPattern = BuildRegex(patternList, position, caseSensitive);
        }

        private static Regex BuildRegex(
            IReadOnlyList<string> patterns,
            MatchPosition position,
            bool caseSensitive)
        {
            var pieces = new List<string>(patterns.Count);

            foreach (string pat in patterns)
            {
                string piece;

                if (position == MatchPosition.Raw)
                {
                    piece = pat;
                }
                else
                {
                    string esc = Regex.Escape(pat);
                    bool isWordOnly = Regex.IsMatch(pat, @"^[\p{L}\p{N}_]+$");

                    piece = position switch
                    {
                        MatchPosition.Nowhere => isWordOnly ? @"\b" + esc + @"\b" : esc,
                        MatchPosition.Prefix => isWordOnly ? @"\b" + esc : esc,
                        MatchPosition.Suffix => isWordOnly ? esc + @"\b" : esc,
                        MatchPosition.Anywhere => esc,
                        _ => esc
                    };
                }

                pieces.Add(piece);
            }

            string combined = "(" + string.Join("|", pieces) + ")";

            RegexOptions options = RegexOptions.Compiled | RegexOptions.CultureInvariant;
            if (!caseSensitive)
                options |= RegexOptions.IgnoreCase;

            return new Regex(combined, options);
        }
    }

    public sealed class RuleBuilder
    {
        private readonly SpeechTranslationSystem _system;
        private readonly List<string> _patterns = new();
        private readonly List<string> _replacements = new();
        private CapitalizationMode _capMode = CapitalizationMode.PerLetter;
        private double _probability = 1.0;
        private bool _applyOnce;
        private MatchPosition _position = MatchPosition.Nowhere;
        private bool _caseSensitive;

        internal RuleBuilder(SpeechTranslationSystem system) => _system = system;

        public RuleBuilder Match(params string[] patterns)
        {
            _patterns.AddRange(patterns);
            return this;
        }

        public RuleBuilder ReplaceWith(params string[] replacements)
        {
            _replacements.AddRange(replacements);
            return this;
        }

        public RuleBuilder At(MatchPosition position)
        {
            _position = position;
            return this;
        }

        public RuleBuilder WithProbability(double probability)
        {
            _probability = probability;
            return this;
        }

        public RuleBuilder WithCapitalization(CapitalizationMode capMode)
        {
            _capMode = capMode;
            return this;
        }

        public RuleBuilder ApplyOnce()
        {
            _applyOnce = true;
            return this;
        }

        public RuleBuilder CaseSensitive()
        {
            _caseSensitive = true;
            return this;
        }

        public void Add()
        {
            if (_patterns.Count == 0)
                throw new InvalidOperationException(
                    "No patterns were specified. Call Match() before Add().");
            if (_replacements.Count == 0)
                throw new InvalidOperationException(
                    "No replacements were specified. Call ReplaceWith() before Add().");

            _system.AddRule(
                _patterns,
                _replacements,
                _capMode,
                _probability,
                _applyOnce,
                _position,
                _caseSensitive);
        }
    }

    public sealed class SpeechTranslationSystem
    {
        private readonly List<ReplacementRule> _rules = new();
        private readonly Random _random;

        public int RuleCount => _rules.Count;
        public bool HasRules => _rules.Count > 0;

        public SpeechTranslationSystem(Random? random = null)
        {
            _random = random ?? new Random();
        }

        public void AddRule(
            string pattern,
            string replacement,
            CapitalizationMode capMode = CapitalizationMode.PerLetter,
            double probability = 1.0,
            bool applyOnce = false,
            MatchPosition position = MatchPosition.Nowhere,
            bool caseSensitive = false)
            => AddRule(new[] { pattern }, new[] { replacement },
                capMode, probability, applyOnce, position, caseSensitive);

        public void AddRule(
            string pattern,
            IEnumerable<string> replacements,
            CapitalizationMode capMode = CapitalizationMode.PerLetter,
            double probability = 1.0,
            bool applyOnce = false,
            MatchPosition position = MatchPosition.Nowhere,
            bool caseSensitive = false)
            => AddRule(new[] { pattern }, replacements,
                capMode, probability, applyOnce, position, caseSensitive);

        public void AddRule(
            IEnumerable<string> patterns,
            string replacement,
            CapitalizationMode capMode = CapitalizationMode.PerLetter,
            double probability = 1.0,
            bool applyOnce = false,
            MatchPosition position = MatchPosition.Nowhere,
            bool caseSensitive = false)
            => AddRule(patterns, new[] { replacement },
                capMode, probability, applyOnce, position, caseSensitive);

        public void AddRule(
            IEnumerable<string> patterns,
            IEnumerable<string> replacements,
            CapitalizationMode capMode = CapitalizationMode.PerLetter,
            double probability = 1.0,
            bool applyOnce = false,
            MatchPosition position = MatchPosition.Nowhere,
            bool caseSensitive = false)
        {
            _rules.Add(new ReplacementRule(
                patterns,
                replacements,
                capMode,
                probability,
                applyOnce,
                position,
                caseSensitive));
        }

        public RuleBuilder NewRule() => new RuleBuilder(this);
        public void ClearRules() => _rules.Clear();
        public void RemoveRuleAt(int index) => _rules.RemoveAt(index);

        public string Process(string input)
        {
            if (_rules.Count == 0 || string.IsNullOrEmpty(input))
                return input;

            string processed = input;

            foreach (var rule in _rules)
            {
                MatchCollection matches = rule.CompiledPattern.Matches(processed);
                if (matches.Count == 0)
                    continue;

                if (rule.ApplyOnce)
                {
                    if (_random.NextDouble() > rule.Probability)
                        continue;

                    Match chosen = matches[_random.Next(matches.Count)];
                    string capitalized = ApplyCapitalization(chosen.Value, PickReplacement(rule, chosen), rule.CapMode);

                    processed = string.Concat(
                        processed.AsSpan(0, chosen.Index),
                        capitalized.AsSpan(),
                        processed.AsSpan(chosen.Index + chosen.Length));

                    continue;
                }

                var sb = new System.Text.StringBuilder(processed.Length);
                int lastIndex = 0;

                foreach (Match match in matches)
                {
                    sb.Append(processed, lastIndex, match.Index - lastIndex);

                    if (_random.NextDouble() <= rule.Probability)
                        sb.Append(ApplyCapitalization(match.Value, PickReplacement(rule, match), rule.CapMode));
                    else
                        sb.Append(match.Value);

                    lastIndex = match.Index + match.Length;
                }

                sb.Append(processed, lastIndex, processed.Length - lastIndex);
                processed = sb.ToString();
            }

            return processed;
        }

        private string PickReplacement(ReplacementRule rule, Match match)
        {
            string replacement = rule.Replacements.Count == 1
                ? rule.Replacements[0]
                : rule.Replacements[_random.Next(rule.Replacements.Count)];

            return match.Result(replacement);
        }

        private static string ApplyCapitalization(
            string original,
            string replacement,
            CapitalizationMode mode)
        {
            if (string.IsNullOrEmpty(replacement))
                return replacement;

            return mode switch
            {
                CapitalizationMode.PerLetter => MapPerLetter(original, replacement),
                CapitalizationMode.FirstLetter => MapFirstLetter(original, replacement),
                CapitalizationMode.AllUpper => replacement.ToUpperInvariant(),
                CapitalizationMode.AllLower => replacement.ToLowerInvariant(),
                CapitalizationMode.Preserve => replacement,
                _ => replacement
            };
        }

        private static string MapFirstLetter(string original, string replacement)
        {
            char? firstLetter = null;
            foreach (char c in original)
            {
                if (char.IsLetter(c)) { firstLetter = c; break; }
            }

            if (firstLetter is null)
                return replacement;

            char mappedFirst = char.IsUpper(firstLetter.Value)
                ? char.ToUpperInvariant(replacement[0])
                : char.ToLowerInvariant(replacement[0]);

            return replacement.Length == 1
                ? mappedFirst.ToString()
                : mappedFirst + replacement.Substring(1);
        }

        private static string MapPerLetter(string original, string replacement)
        {
            char[] result = new char[replacement.Length];
            int origIdx = 0;
            bool lastWasUpper = false;
            bool hasMappedAny = false;

            for (int i = 0; i < replacement.Length; i++)
            {
                char rc = replacement[i];

                if (!char.IsLetter(rc))
                {
                    result[i] = rc;
                    continue;
                }

                while (origIdx < original.Length && !char.IsLetter(original[origIdx]))
                    origIdx++;

                if (origIdx < original.Length)
                {
                    lastWasUpper = char.IsUpper(original[origIdx]);
                    hasMappedAny = true;
                    origIdx++;
                }

                result[i] = hasMappedAny && lastWasUpper
                    ? char.ToUpperInvariant(rc)
                    : char.ToLowerInvariant(rc);
            }

            return new string(result);
        }
    }
}
