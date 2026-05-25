using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Robust.Shared.Utility;

namespace Content.Shared.Localizations
{
    public sealed class ContentLocalizationManager
    {
        [Dependency] private readonly ILocalizationManager _loc = default!;

        // UKRAINIAN LOCALIZATION: Set Ukrainian as primary culture with English fallback
        private const string Culture = "uk-UA";
        private const string FallbackCulture = "en-US";

        /// <summary>
        /// Custom format strings used for parsing and displaying minutes:seconds timespans.
        /// </summary>
        public static readonly string[] TimeSpanMinutesFormats = new string[]
        {
            @"m\:ss",
            @"mm\:ss",
            @"%m",
            @"mm"
        };

        public void Initialize()
        {
            // Load Ukrainian culture as primary
            var culture = new CultureInfo(Culture);
            if (!_loc.HasCulture(culture))
                _loc.LoadCulture(culture);

            // Load English as fallback
            var fallbackCulture = new CultureInfo(FallbackCulture);
            if (!_loc.HasCulture(fallbackCulture))
                _loc.LoadCulture(fallbackCulture);

            _loc.SetCulture(culture);

            // Add functions for Ukrainian culture
            _loc.AddFunction(culture, "PRESSURE", FormatPressure);
            _loc.AddFunction(culture, "POWERWATTS", FormatPowerWatts);
            _loc.AddFunction(culture, "POWERJOULES", FormatPowerJoules);
            _loc.AddFunction(culture, "ENERGYWATTHOURS", FormatEnergyWattHours);
            _loc.AddFunction(culture, "UNITS", FormatUnits);
            _loc.AddFunction(culture, "TOSTRING", args => FormatToString(culture, args));
            _loc.AddFunction(culture, "NATURALFIXED", FormatNaturalFixed);
            _loc.AddFunction(culture, "NATURALPERCENT", FormatNaturalPercent);
            _loc.AddFunction(culture, "PLAYTIME", FormatPlaytime);

            // Ukrainian-specific functions
            _loc.AddFunction(culture, "UKPLURAL", FormatUkrainianPlural);
            _loc.AddFunction(culture, "UKGENDER", FormatUkrainianGender);
            _loc.AddFunction(culture, "UKCASE", FormatUkrainianCase);
            _loc.AddFunction(culture, "UKPLURALGEN", FormatUkrainianPluralWithGender);
            _loc.AddFunction(culture, "UKTIME", FormatUkrainianTime);
            _loc.AddFunction(culture, "UKLIST", FormatUkrainianList);
            _loc.AddFunction(culture, "UKNAME", FormatUkrainianName);

            // Ukrainian equivalents of English grammar functions
            _loc.AddFunction(culture, "SUBJECT", FormatUkrainianSubject);
            _loc.AddFunction(culture, "OBJECT", FormatUkrainianObject);
            _loc.AddFunction(culture, "POSS-ADJ", FormatUkrainianPossessiveAdjective);
            _loc.AddFunction(culture, "POSS-PRONOUN", FormatUkrainianPossessivePronoun);
            _loc.AddFunction(culture, "REFLEXIVE", FormatUkrainianReflexive);
            _loc.AddFunction(culture, "CONJUGATE-BE", FormatUkrainianConjugateBe);
            _loc.AddFunction(culture, "CONJUGATE-BASIC", FormatUkrainianConjugateBasic);
            _loc.AddFunction(culture, "PROPER", FormatUkrainianProper);
            _loc.AddFunction(culture, "MAKEPLURAL", FormatMakePlural);
            _loc.AddFunction(culture, "MANY", FormatMany);

            // Add basic functions to fallback culture
            _loc.AddFunction(fallbackCulture, "PRESSURE", FormatPressure);
            _loc.AddFunction(fallbackCulture, "POWERWATTS", FormatPowerWatts);
            _loc.AddFunction(fallbackCulture, "POWERJOULES", FormatPowerJoules);
            _loc.AddFunction(fallbackCulture, "ENERGYWATTHOURS", FormatEnergyWattHours);
            _loc.AddFunction(fallbackCulture, "UNITS", FormatUnits);
            _loc.AddFunction(fallbackCulture, "TOSTRING", args => FormatToString(fallbackCulture, args));
            _loc.AddFunction(fallbackCulture, "NATURALFIXED", FormatNaturalFixed);
            _loc.AddFunction(fallbackCulture, "NATURALPERCENT", FormatNaturalPercent);
            _loc.AddFunction(fallbackCulture, "PLAYTIME", FormatPlaytime);
            _loc.AddFunction(fallbackCulture, "MAKEPLURAL", FormatMakePlural);
            _loc.AddFunction(fallbackCulture, "MANY", FormatMany);
        }

        private ILocValue FormatMany(LocArgs args)
        {
            var count = ((LocValueNumber) args.Args[1]).Value;

            if (System.Math.Abs(count - 1) < 0.0001f)
            {
                return (LocValueString) args.Args[0];
            }
            else
            {
                return (LocValueString) FormatMakePlural(args);
            }
        }

        private ILocValue FormatNaturalPercent(LocArgs args)
        {
            var number = ((LocValueNumber) args.Args[0]).Value * 100;
            var maxDecimals = (int)System.Math.Floor(((LocValueNumber) args.Args[1]).Value);
            var formatter = (NumberFormatInfo)NumberFormatInfo.GetInstance(CultureInfo.GetCultureInfo(Culture)).Clone();
            formatter.NumberDecimalDigits = maxDecimals;
            string formatted = string.Format(formatter, "{0:N}", number);
            string separator = formatter.NumberDecimalSeparator;
            string trimmed = formatted.TrimEnd('0').TrimEnd(separator[0]);
            return new LocValueString(trimmed + "%");
        }

        private ILocValue FormatNaturalFixed(LocArgs args)
        {
            var number = ((LocValueNumber) args.Args[0]).Value;
            var maxDecimals = (int)System.Math.Floor(((LocValueNumber) args.Args[1]).Value);
            var formatter = (NumberFormatInfo)NumberFormatInfo.GetInstance(CultureInfo.GetCultureInfo(Culture)).Clone();
            formatter.NumberDecimalDigits = maxDecimals;
            string formatted = string.Format(formatter, "{0:N}", number);
            string separator = formatter.NumberDecimalSeparator;
            return new LocValueString(formatted.TrimEnd('0').TrimEnd(separator[0]));
        }

        private static readonly Regex PluralEsRule = new("^.*(s|sh|ch|x|z)$");

        private ILocValue FormatMakePlural(LocArgs args)
        {
            var text = ((LocValueString) args.Args[0]).Value;
            var split = text.Split(" ", 1);
            var firstWord = split[0];
            if (PluralEsRule.IsMatch(firstWord))
            {
                if (split.Length == 1)
                    return new LocValueString(firstWord + "es");
                else
                    return new LocValueString(firstWord + "es " + split[1]);
            }
            else
            {
                if (split.Length == 1)
                    return new LocValueString(firstWord + "s");
                else
                    return new LocValueString(firstWord + "s " + split[1]);
            }
        }

        public static string FormatList(System.Collections.Generic.List<string> list)
        {
            if (list.Count <= 0) return string.Empty;
            if (list.Count == 1) return list[0];
            if (list.Count == 2) return list[0] + " and " + list[1];
            
            string combined = string.Join(", ", list.GetRange(0, list.Count - 1));
            return combined + ", and " + list[list.Count - 1];
        }

        public static string FormatListToOr(System.Collections.Generic.List<string> list)
        {
            if (list.Count <= 0) return string.Empty;
            if (list.Count == 1) return list[0];
            if (list.Count == 2) return list[0] + " or " + list[1];
            
            string combined = string.Join(", ", list.GetRange(0, list.Count - 1));
            return combined + ", or " + list[list.Count - 1];
        }

        public static string FormatDirection(Direction dir)
        {
            return Loc.GetString("zzzz-fmt-direction-" + dir.ToString());
        }

        public static string FormatPlaytime(TimeSpan time)
        {
            time = TimeSpan.FromMinutes(System.Math.Ceiling(time.TotalMinutes));
            var hours = (int)time.TotalHours;
            var minutes = time.Minutes;
            
            var locArgs = new (string, object)[] { ("hours", hours), ("minutes", minutes) };
            return Loc.GetString("zzzz-fmt-playtime", locArgs);
        }

        private static ILocValue FormatToString(CultureInfo culture, LocArgs args)
        {
            var arg = args.Args[0];
            var fmt = ((LocValueString) args.Args[1]).Value;

            var obj = arg.Value;
            if (obj is IFormattable formattable)
                return new LocValueString(formattable.ToString(fmt, culture));

            return new LocValueString(obj?.ToString() ?? "");
        }

        private static ILocValue FormatUnitsGeneric(
            LocArgs args,
            string mode,
            System.Func<double, double>? transformValue = null)
        {
            const int maxPlaces = 5; // Matches amount in _lib.ftl
            var pressure = ((LocValueNumber) args.Args[0]).Value;

            if (transformValue != null)
                pressure = transformValue(pressure);

            var places = 0;
            while (pressure > 1000 && places < maxPlaces)
            {
                pressure /= 1000;
                places += 1;
            }

            var locArgs = new (string, object)[] { ("divided", pressure), ("places", places) };
            return new LocValueString(Loc.GetString(mode, locArgs));
        }

        private static ILocValue FormatPressure(LocArgs args)
        {
            return FormatUnitsGeneric(args, "zzzz-fmt-pressure");
        }

        private static ILocValue FormatPowerWatts(LocArgs args)
        {
            return FormatUnitsGeneric(args, "zzzz-fmt-power-watts");
        }

        private static ILocValue FormatPowerJoules(LocArgs args)
        {
            return FormatUnitsGeneric(args, "zzzz-fmt-power-joules");
        }

        private static ILocValue FormatEnergyWattHours(LocArgs args)
        {
            const double joulesToWattHours = 1.0 / 3600;

            return FormatUnitsGeneric(args, "zzzz-fmt-energy-watt-hours", joules => joules * joulesToWattHours);
        }

        private static ILocValue FormatUnits(LocArgs args)
        {
            string unitType = ((LocValueString) args.Args[0]).Value;
            if (!Units.Types.TryGetValue(unitType, out var ut))
                throw new System.ArgumentException("Unknown unit type " + unitType);

            var fmtstr = ((LocValueString) args.Args[1]).Value;

            double max = System.Double.NegativeInfinity;
            var iargs = new double[args.Args.Count - 1];
            for (var i = 2; i < args.Args.Count; i++)
            {
                var n = ((LocValueNumber) args.Args[i]).Value;
                if (n > max)
                    max = n;

                iargs[i - 2] = n;
            }

            if (!ut.TryGetUnit(max, out var mu))
                throw new System.ArgumentException("Unit out of range for type");

            var fargs = new object[iargs.Length];

            for (var i = 0; i < iargs.Length; i++)
                fargs[i] = iargs[i] * mu.Factor;

            fargs[fargs.Length - 1] = Loc.GetString("units-" + mu.Unit.ToLower());

            var res = System.String.Format(
                fmtstr.Replace("{UNIT", "{" + (fargs.Length - 1).ToString()),
                fargs
            );

            return new LocValueString(res);
        }

        private static ILocValue FormatPlaytime(LocArgs args)
        {
            var time = TimeSpan.Zero;
            if (args.Args.Count > 0 && args.Args[0].Value is TimeSpan timeArg)
            {
                time = timeArg;
            }
            return new LocValueString(FormatPlaytime(time));
        }

        private ILocValue FormatUkrainianPlural(LocArgs args)
        {
            var count = (int)System.Math.Abs(((LocValueNumber)args.Args[0]).Value);
            var form1 = ((LocValueString)args.Args[1]).Value;
            var form2 = ((LocValueString)args.Args[2]).Value;
            var form5 = ((LocValueString)args.Args[3]).Value;

            var lastDigit = count % 10;
            var lastTwoDigits = count % 100;

            if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
            {
                return new LocValueString(form5);
            }
            if (lastDigit == 1)
            {
                return new LocValueString(form1);
            }
            if (lastDigit >= 2 && lastDigit <= 4)
            {
                return new LocValueString(form2);
            }

            return new LocValueString(form5);
        }

        private ILocValue FormatUkrainianGender(LocArgs args)
        {
            var gender = ((LocValueString)args.Args[0]).Value.ToLower();
            var masculine = ((LocValueString)args.Args[1]).Value;
            var feminine = ((LocValueString)args.Args[2]).Value;
            var neuter = ((LocValueString)args.Args[3]).Value;

            if (gender == "male" || gender == "masculine" || gender == "m")
                return new LocValueString(masculine);
            if (gender == "female" || gender == "feminine" || gender == "f")
                return new LocValueString(feminine);
            if (gender == "neuter" || gender == "n")
                return new LocValueString(neuter);
            
            return new LocValueString(masculine);
        }

        private ILocValue FormatUkrainianCase(LocArgs args)
        {
            var caseType = ((LocValueString)args.Args[0]).Value.ToLower();

            var nominative = ((LocValueString)args.Args[1]).Value;
            var genitive = args.Args.Count > 2 ? ((LocValueString)args.Args[2]).Value : nominative;
            var dative = args.Args.Count > 3 ? ((LocValueString)args.Args[3]).Value : nominative;
            var accusative = args.Args.Count > 4 ? ((LocValueString)args.Args[4]).Value : nominative;
            var instrumental = args.Args.Count > 5 ? ((LocValueString)args.Args[5]).Value : nominative;
            var locative = args.Args.Count > 6 ? ((LocValueString)args.Args[6]).Value : nominative;

            if (caseType == "nominative" || caseType == "nom" || caseType == "називний")
                return new LocValueString(nominative);
            if (caseType == "genitive" || caseType == "gen" || caseType == "родовий")
                return new LocValueString(genitive);
            if (caseType == "dative" || caseType == "dat" || caseType == "давальний")
                return new LocValueString(dative);
            if (caseType == "accusative" || caseType == "acc" || caseType == "знахідний")
                return new LocValueString(accusative);
            if (caseType == "instrumental" || caseType == "ins" || caseType == "орудний")
                return new LocValueString(instrumental);
            if (caseType == "locative" || caseType == "loc" || caseType == "місцевий")
                return new LocValueString(locative);
            
            return new LocValueString(nominative);
        }

        private ILocValue FormatUkrainianPluralWithGender(LocArgs args)
        {
            var count = (int)System.Math.Abs(((LocValueNumber)args.Args[0]).Value);
            var gender = ((LocValueString)args.Args[1]).Value.ToLower();

            var singular = ((LocValueString)args.Args[2]).Value;
            var plural24 = ((LocValueString)args.Args[3]).Value;
            var plural5 = ((LocValueString)args.Args[4]).Value;

            var lastDigit = count % 10;
            var lastTwoDigits = count % 100;

            string selectedForm;
            if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
                selectedForm = plural5;
            else if (lastDigit == 1)
                selectedForm = singular;
            else if (lastDigit >= 2 && lastDigit <= 4)
                selectedForm = plural24;
            else
                selectedForm = plural5;

            var parts = selectedForm.Split('/');
            if (parts.Length == 3)
            {
                if (gender == "male" || gender == "masculine" || gender == "m")
                    return new LocValueString(parts[0]);
                if (gender == "female" || gender == "feminine" || gender == "f")
                    return new LocValueString(parts[1]);
                if (gender == "neuter" || gender == "n")
                    return new LocValueString(parts[2]);
            }

            return new LocValueString(selectedForm);
        }

        private ILocValue FormatUkrainianTime(LocArgs args)
        {
            var hours = (int)System.Math.Abs(((LocValueNumber)args.Args[0]).Value);
            var minutes = (int)System.Math.Abs(((LocValueNumber)args.Args[1]).Value);

            var hoursWord = GetUkrainianPluralForm(hours, "година", "години", "годин");
            var minutesWord = GetUkrainianPluralForm(minutes, "хвилина", "хвилини", "хвилин");

            if (hours > 0 && minutes > 0)
                return new LocValueString(hours.ToString() + " " + hoursWord + " " + minutes.ToString() + " " + minutesWord);
            if (hours > 0)
                return new LocValueString(hours.ToString() + " " + hoursWord);
            if (minutes > 0)
                return new LocValueString(minutes.ToString() + " " + minutesWord);
            
            return new LocValueString("0 хвилин");
        }

        private ILocValue FormatUkrainianList(LocArgs args)
        {
            var items = new System.Collections.Generic.List<string>();
            foreach (var arg in args.Args)
            {
                items.Add(((LocValueString)arg).Value);
            }

            if (items.Count <= 0)
                return new LocValueString(string.Empty);
            if (items.Count == 1)
                return new LocValueString(items[0]);
            if (items.Count == 2)
                return new LocValueString(items[0] + " та " + items[1]);
            
            string combined = string.Join(", ", items.GetRange(0, items.Count - 1));
            return new LocValueString(combined + " та " + items[items.Count - 1]);
        }

        private string GetUkrainianPluralForm(int count, string form1, string form2, string form5)
        {
            var lastDigit = count % 10;
            var lastTwoDigits = count % 100;

            if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
                return form5;
            if (lastDigit == 1)
                return form1;
            if (lastDigit >= 2 && lastDigit <= 4)
                return form2;
            return form5;
        }

        private ILocValue FormatUkrainianName(LocArgs args)
        {
            var caseType = ((LocValueString)args.Args[0]).Value.ToLower();
            var name = ((LocValueString)args.Args[1]).Value;

            if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
                return new LocValueString(name);

            var parts = name.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            var declinedParts = new System.Collections.Generic.List<string>();

            foreach (var part in parts)
            {
                declinedParts.Add(DeclineUkrainianWord(part, caseType));
            }

            return new LocValueString(string.Join(" ", declinedParts));
        }

        private string DeclineUkrainianWord(string word, string caseType)
        {
            if (string.IsNullOrWhiteSpace(word) || word.Length < 2)
                return word;

            if (word.Contains("-"))
            {
                var parts = word.Split('-');
                var lastPart = parts[parts.Length - 1];
                var declinedLastPart = DeclineUkrainianWord(lastPart, caseType);
                parts[parts.Length - 1] = declinedLastPart;
                return string.Join("-", parts);
            }

            var lower = word.ToLower();
            char lastChar = lower[lower.Length - 1];

            if (caseType == "nominative" || caseType == "nom" || caseType == "називний")
                return word;

            if (caseType == "genitive" || caseType == "gen" || caseType == "родовий")
            {
                if (IsConsonant(lastChar)) return word + "а";
                if (lastChar == 'о') return word.Substring(0, word.Length - 1) + "а";
                if (lastChar == 'а') return word.Substring(0, word.Length - 1) + "і";
                if (lastChar == 'я') return word.Substring(0, word.Length - 1) + "ї";
                return word;
            }

            if (caseType == "dative" || caseType == "dat" || caseType == "давальний")
            {
                if (IsConsonant(lastChar)) return word + "у";
                if (lastChar == 'о') return word.Substring(0, word.Length - 1) + "у";
                if (lastChar == 'а') return word.Substring(0, word.Length - 1) + "і";
                if (lastChar == 'я') return word.Substring(0, word.Length - 1) + "ї";
                return word;
            }

            if (caseType == "accusative" || caseType == "acc" || caseType == "знахідний")
            {
                if (IsConsonant(lastChar)) return word + "а";
                if (lastChar == 'о') return word.Substring(0, word.Length - 1) + "а";
                if (lastChar == 'а') return word.Substring(0, word.Length - 1) + "ю";
                if (lastChar == 'я') return word.Substring(0, word.Length - 1) + "ю";
                return word;
            }

            if (caseType == "instrumental" || caseType == "ins" || caseType == "орудний")
            {
                if (IsConsonant(lastChar)) return word + "ом";
                if (lastChar == 'о') return word.Substring(0, word.Length - 1) + "ом";
                if (lastChar == 'а') return word.Substring(0, word.Length - 1) + "єю";
                if (lastChar == 'я') return word.Substring(0, word.Length - 1) + "єю";
                return word;
            }

            if (caseType == "locative" || caseType == "loc" || caseType == "місцевий")
            {
                if (IsConsonant(lastChar)) return word + "ові";
                if (lastChar == 'о') return word.Substring(0, word.Length - 1) + "ові";
                if (lastChar == 'а') return word.Substring(0, word.Length - 1) + "і";
                if (lastChar == 'я') return word.Substring(0, word.Length - 1) + "ї";
                return word;
            }

            return word;
        }

        private bool IsConsonant(char c)
        {
            char lc = char.ToLower(c);
            if (lc == 'а') return false;
            if (lc == 'е') return false;
            if (lc == 'є') return false;
            if (lc == 'и') return false;
            if (lc == 'і') return false;
            if (lc == 'ї') return false;
            if (lc == 'о') return false;
            if (lc == 'у') return false;
            if (lc == 'ю') return false;
            if (lc == 'я') return false;
            return true;
        }

        private ILocValue FormatUkrainianSubject(LocArgs args)
        {
            var name = ((LocValueString)args.Args[0]).Value;
            return new LocValueString(name);
        }

        private ILocValue FormatUkrainianObject(LocArgs args)
        {
            var name = ((LocValueString)args.Args[0]).Value;
            return new LocValueString(DeclineUkrainianWord(name, "accusative"));
        }

        private ILocValue FormatUkrainianPossessiveAdjective(LocArgs args)
        {
            return new LocValueString("його");
        }

        private ILocValue FormatUkrainianPossessivePronoun(LocArgs args)
        {
            return new LocValueString("його");
        }

        private ILocValue FormatUkrainianReflexive(LocArgs args)
        {
            return new LocValueString("себе");
        }

        private ILocValue FormatUkrainianConjugateBe(LocArgs args)
        {
            return new LocValueString("є");
        }

        private ILocValue FormatUkrainianConjugateBasic(LocArgs args)
        {
            var singular = ((LocValueString)args.Args[1]).Value;
            return new LocValueString(singular);
        }

        private ILocValue FormatUkrainianProper(LocArgs args)
        {
            var name = ((LocValueString)args.Args[0]).Value;
            if (string.IsNullOrEmpty(name))
                return new LocValueString("");

            string first = name.Substring(0, 1).ToUpper();
            if (name.Length == 1)
                return new LocValueString(first);

            string rest = name.Substring(1);
            return new LocValueString(first + rest);
        }
    }
}
