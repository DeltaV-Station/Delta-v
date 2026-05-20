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
        public static readonly string[] TimeSpanMinutesFormats = new[]
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
            _loc.AddFunction(culture, "LOC", FormatLoc);
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

            // Add functions for English fallback culture
            _loc.AddFunction(fallbackCulture, "PRESSURE", FormatPressure);
            _loc.AddFunction(fallbackCulture, "POWERWATTS", FormatPowerWatts);
            _loc.AddFunction(fallbackCulture, "POWERJOULES", FormatPowerJoules);
            _loc.AddFunction(fallbackCulture, "ENERGYWATTHOURS", FormatEnergyWattHours);
            _loc.AddFunction(fallbackCulture, "UNITS", FormatUnits);
            _loc.AddFunction(fallbackCulture, "TOSTRING", args => FormatToString(fallbackCulture, args));
            _loc.AddFunction(fallbackCulture, "LOC", FormatLoc);
            _loc.AddFunction(fallbackCulture, "NATURALFIXED", FormatNaturalFixed);
            _loc.AddFunction(fallbackCulture, "NATURALPERCENT", FormatNaturalPercent);
            _loc.AddFunction(fallbackCulture, "PLAYTIME", FormatPlaytime);
            _loc.AddFunction(fallbackCulture, "MAKEPLURAL", FormatMakePlural);
            _loc.AddFunction(fallbackCulture, "MANY", FormatMany);
        }

        private ILocValue FormatMany(LocArgs args)
        {
            var count = ((LocValueNumber) args.Args[1]).Value;

            if (Math.Abs(count - 1) < 0.0001f)
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
            var maxDecimals = (int)Math.Floor(((LocValueNumber) args.Args[1]).Value);
            var formatter = (NumberFormatInfo)NumberFormatInfo.GetInstance(CultureInfo.GetCultureInfo(Culture)).Clone();
            formatter.NumberDecimalDigits = maxDecimals;
            return new LocValueString(string.Format(formatter, "{0:N}", number).TrimEnd('0').TrimEnd(char.Parse(formatter.NumberDecimalSeparator)) + "%");
        }

        private ILocValue FormatNaturalFixed(LocArgs args)
        {
            var number = ((LocValueNumber) args.Args[0]).Value;
            var maxDecimals = (int)Math.Floor(((LocValueNumber) args.Args[1]).Value);
            var formatter = (NumberFormatInfo)NumberFormatInfo.GetInstance(CultureInfo.GetCultureInfo(Culture)).Clone();
            formatter.NumberDecimalDigits = maxDecimals;
            return new LocValueString(string.Format(formatter, "{0:N}", number).TrimEnd('0').TrimEnd(char.Parse(formatter.NumberDecimalSeparator)));
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
                    return new LocValueString($"{firstWord}es");
                else
                    return new LocValueString($"{firstWord}es {split[1]}");
            }
            else
            {
                if (split.Length == 1)
                    return new LocValueString($"{firstWord}s");
                else
                    return new LocValueString($"{firstWord}s {split[1]}");
            }
        }

        // TODO: allow fluent to take in lists of strings so this can be a format function like it should be.
        /// <summary>
        /// Formats a list as per english grammar rules.
        /// </summary>
        public static string FormatList(List<string> list)
        {
            return list.Count switch
            {
                <= 0 => string.Empty,
                1 => list[0],
                2 => $"{list[0]} and {list[1]}",
                _ => $"{string.Join(", ", list.GetRange(0, list.Count - 1))}, and {list[^1]}"
            };
        }

        /// <summary>
        /// Formats a list as per english grammar rules, but uses or instead of and.
        /// </summary>
        public static string FormatListToOr(List<string> list)
        {
            return list.Count switch
            {
                <= 0 => string.Empty,
                1 => list[0],
                2 => $"{list[0]} or {list[1]}",
                _ => $"{string.Join(", ", list.GetRange(0, list.Count - 1))}, or {list[^1]}"
            };
        }

        /// <summary>
        /// Formats a direction struct as a human-readable string.
        /// </summary>
        public static string FormatDirection(Direction dir)
        {
            return Loc.GetString($"zzzz-fmt-direction-{dir.ToString()}");
        }

        /// <summary>
        /// Formats playtime as hours and minutes.
        /// </summary>
        public static string FormatPlaytime(TimeSpan time)
        {
            time = TimeSpan.FromMinutes(Math.Ceiling(time.TotalMinutes));
            var hours = (int)time.TotalHours;
            var minutes = time.Minutes;
            return Loc.GetString($"zzzz-fmt-playtime", ("hours", hours), ("minutes", minutes));
        }

        private static ILocValue FormatLoc(LocArgs args)
        {
            var id = ((LocValueString) args.Args[0]).Value;

            return new LocValueString(Loc.GetString(id, args.Options.Select(x => (x.Key, x.Value.Value!)).ToArray()));
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
            Func<double, double>? transformValue = null)
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

            return new LocValueString(Loc.GetString(mode, ("divided", pressure), ("places", places)));
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
            if (!Units.Types.TryGetValue(((LocValueString) args.Args[0]).Value, out var ut))
                throw new ArgumentException($"Unknown unit type {((LocValueString) args.Args[0]).Value}");

            var fmtstr = ((LocValueString) args.Args[1]).Value;

            double max = Double.NegativeInfinity;
            var iargs = new double[args.Args.Count - 1];
            for (var i = 2; i < args.Args.Count; i++)
            {
                var n = ((LocValueNumber) args.Args[i]).Value;
                if (n > max)
                    max = n;

                iargs[i - 2] = n;
            }

            if (!ut.TryGetUnit(max, out var mu))
                throw new ArgumentException("Unit out of range for type");

            var fargs = new object[iargs.Length];

            for (var i = 0; i < iargs.Length; i++)
                fargs[i] = iargs[i] * mu.Factor;

            fargs[^1] = Loc.GetString($"units-{mu.Unit.ToLower()}");

            // Before anyone complains about "{"+"${...}", at least it's better than MS's approach...
            // https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting#escaping-braces
            //
            // Note that the closing brace isn't replaced so that format specifiers can be applied.
            var res = String.Format(
                fmtstr.Replace("{UNIT", "{" + $"{fargs.Length - 1}"),
                fargs
            );

            return new LocValueString(res);
        }

        private static ILocValue FormatPlaytime(LocArgs args)
        {
            var time = TimeSpan.Zero;
            if (args.Args is { Count: > 0 } && args.Args[0].Value is TimeSpan timeArg)
            {
                time = timeArg;
            }
            return new LocValueString(FormatPlaytime(time));
        }

        /// <summary>
        /// Форматує множину за українськими правилами
        /// Використання: UKPLURAL($count, "предмет", "предмети", "предметів")
        /// </summary>
        private ILocValue FormatUkrainianPlural(LocArgs args)
        {
            var count = (int)Math.Abs(((LocValueNumber)args.Args[0]).Value);
            var form1 = ((LocValueString)args.Args[1]).Value; // 1 предмет
            var form2 = ((LocValueString)args.Args[2]).Value; // 2-4 предмети
            var form5 = ((LocValueString)args.Args[3]).Value; // 5+ предметів

            var lastDigit = count % 10;
            var lastTwoDigits = count % 100;

            // Правила української множини:
            // 1, 21, 31, 41... - форма 1 (предмет)
            // 2-4, 22-24, 32-34... - форма 2-4 (предмети)
            // 5-20, 25-30, 35-40... - форма 5+ (предметів)
            // 11-14 - виняток, завжди форма 5+ (предметів)

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

        /// <summary>
        /// Форматує слово за родом
        /// Використання: UKGENDER($gender, "він", "вона", "воно")
        /// </summary>
        private ILocValue FormatUkrainianGender(LocArgs args)
        {
            var gender = ((LocValueString)args.Args[0]).Value.ToLower();
            var masculine = ((LocValueString)args.Args[1]).Value;
            var feminine = ((LocValueString)args.Args[2]).Value;
            var neuter = ((LocValueString)args.Args[3]).Value;

            return gender switch
            {
                "male" or "masculine" or "m" => new LocValueString(masculine),
                "female" or "feminine" or "f" => new LocValueString(feminine),
                "neuter" or "n" => new LocValueString(neuter),
                _ => new LocValueString(masculine)
            };
        }

        /// <summary>
        /// Форматує слово за відмінком (спрощена версія)
        /// Використання: UKCASE($case, "називний", "родовий", "давальний", "знахідний", "орудний", "місцевий")
        /// </summary>
        private ILocValue FormatUkrainianCase(LocArgs args)
        {
            var caseType = ((LocValueString)args.Args[0]).Value.ToLower();

            // Відмінки: nominative, genitive, dative, accusative, instrumental, locative
            var nominative = ((LocValueString)args.Args[1]).Value;   // називний (хто? що?)
            var genitive = args.Args.Count > 2 ? ((LocValueString)args.Args[2]).Value : nominative;     // родовий (кого? чого?)
            var dative = args.Args.Count > 3 ? ((LocValueString)args.Args[3]).Value : nominative;       // давальний (кому? чому?)
            var accusative = args.Args.Count > 4 ? ((LocValueString)args.Args[4]).Value : nominative;   // знахідний (кого? що?)
            var instrumental = args.Args.Count > 5 ? ((LocValueString)args.Args[5]).Value : nominative; // орудний (ким? чим?)
            var locative = args.Args.Count > 6 ? ((LocValueString)args.Args[6]).Value : nominative;     // місцевий (на кому? на чому?)

            return caseType switch
            {
                "nominative" or "nom" or "називний" => new LocValueString(nominative),
                "genitive" or "gen" or "родовий" => new LocValueString(genitive),
                "dative" or "dat" or "давальний" => new LocValueString(dative),
                "accusative" or "acc" or "знахідний" => new LocValueString(accusative),
                "instrumental" or "ins" or "орудний" => new LocValueString(instrumental),
                "locative" or "loc" or "місцевий" => new LocValueString(locative),
                _ => new LocValueString(nominative)
            };
        }

        /// <summary>
        /// Комбінована функція: множина + рід
        /// Використання: UKPLURALGEN($count, $gender, "взяв/взяла/взяло", "взяли/взяли/взяли", "взяли/взяли/взяли")
        /// </summary>
        private ILocValue FormatUkrainianPluralWithGender(LocArgs args)
        {
            var count = (int)Math.Abs(((LocValueNumber)args.Args[0]).Value);
            var gender = ((LocValueString)args.Args[1]).Value.ToLower();

            // Форми для однини (розділені слешем: чоловічий/жіночий/середній)
            var singular = ((LocValueString)args.Args[2]).Value;
            // Форми для 2-4
            var plural24 = ((LocValueString)args.Args[3]).Value;
            // Форми для 5+
            var plural5 = ((LocValueString)args.Args[4]).Value;

            var lastDigit = count % 10;
            var lastTwoDigits = count % 100;

            string selectedForm;
            if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
            {
                selectedForm = plural5;
            }
            else if (lastDigit == 1)
            {
                selectedForm = singular;
            }
            else if (lastDigit >= 2 && lastDigit <= 4)
            {
                selectedForm = plural24;
            }
            else
            {
                selectedForm = plural5;
            }

            // Розбираємо форму за родом (якщо є слеші)
            var parts = selectedForm.Split('/');
            if (parts.Length == 3)
            {
                return gender switch
                {
                    "male" or "masculine" or "m" => new LocValueString(parts[0]),
                    "female" or "feminine" or "f" => new LocValueString(parts[1]),
                    "neuter" or "n" => new LocValueString(parts[2]),
                    _ => new LocValueString(parts[0])
                };
            }

            return new LocValueString(selectedForm);
        }

        /// <summary>
        /// Форматує час у годинах та хвилинах з правильними закінченнями
        /// Використання: UKTIME($hours, $minutes)
        /// </summary>
        private ILocValue FormatUkrainianTime(LocArgs args)
        {
            var hours = (int)Math.Abs(((LocValueNumber)args.Args[0]).Value);
            var minutes = (int)Math.Abs(((LocValueNumber)args.Args[1]).Value);

            var hoursWord = GetUkrainianPluralForm(hours, "година", "години", "годин");
            var minutesWord = GetUkrainianPluralForm(minutes, "хвилина", "хвилини", "хвилин");

            if (hours > 0 && minutes > 0)
                return new LocValueString($"{hours} {hoursWord} {minutes} {minutesWord}");
            else if (hours > 0)
                return new LocValueString($"{hours} {hoursWord}");
            else if (minutes > 0)
                return new LocValueString($"{minutes} {minutesWord}");
            else
                return new LocValueString("0 хвилин");
        }

        /// <summary>
        /// Форматує список українською мовою
        /// Використання: UKLIST("item1", "item2", "item3", ...)
        /// </summary>
        private ILocValue FormatUkrainianList(LocArgs args)
        {
            var items = args.Args.Select(arg => ((LocValueString)arg).Value).ToList();

            return items.Count switch
            {
                <= 0 => new LocValueString(string.Empty),
                1 => new LocValueString(items[0]),
                2 => new LocValueString($"{items[0]} та {items[1]}"),
                _ => new LocValueString($"{string.Join(", ", items.GetRange(0, items.Count - 1))} та {items[^1]}")
            };
        }

        /// <summary>
        /// Допоміжна функція для отримання правильної форми множини
        /// </summary>
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

        /// <summary>
        /// Автоматично відмінює українські імена
        /// Використання: UKNAME($case, $name)
        /// </summary>
        private ILocValue FormatUkrainianName(LocArgs args)
        {
            var caseType = ((LocValueString)args.Args[0]).Value.ToLower();
            var name = ((LocValueString)args.Args[1]).Value;

            // Якщо ім'я порожнє або дуже коротке
            if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
                return new LocValueString(name);

            // Розбиваємо на слова (ім'я та прізвище)
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var declinedParts = new List<string>();

            foreach (var part in parts)
            {
                declinedParts.Add(DeclineUkrainianWord(part, caseType));
            }

            return new LocValueString(string.Join(" ", declinedParts));
        }

        /// <summary>
        /// Відмінює одне українське слово (ім'я або прізвище)
        /// Підтримує імена з дефісом (наприклад, унаті: "Ssi-Ka" -> "Ssi-Ку")
        /// </summary>
        private string DeclineUkrainianWord(string word, string caseType)
        {
            if (string.IsNullOrWhiteSpace(word) || word.Length < 2)
                return word;

            // Якщо ім'я містить дефіс (наприклад, унаті), відмінюємо тільки останню частину
            if (word.Contains('-'))
            {
                var parts = word.Split('-');
                var lastPart = parts[^1];
                var declinedLastPart = DeclineUkrainianWord(lastPart, caseType);
                parts[^1] = declinedLastPart;
                return string.Join("-", parts);
            }

            var lower = word.ToLower();
            var lastChar = lower[^1];
            var lastTwoChars = lower.Length >= 2 ? lower[^2..] : "";

            // Називний відмінок - без змін
            if (caseType == "nominative" || caseType == "nom" || caseType == "називний")
                return word;

            // Родовий відмінок (кого? чого?)
            if (caseType == "genitive" || caseType == "gen" || caseType == "родовий")
            {
                // Чоловічі імена на приголосний: Іван -> Івана
                if (IsConsonant(lastChar))
                    return word + "а";
                // Імена на -о: Петро -> Петра
                if (lastChar == 'о')
                    return word[..^1] + "а";
                // Імена на -а: Марія -> Марії
                if (lastChar == 'а')
                    return word[..^1] + "і";
                // Імена на -я: Софія -> Софії
                if (lastChar == 'я')
                    return word[..^1] + "ї";
                return word;
            }

            // Давальний відмінок (кому? чому?)
            if (caseType == "dative" || caseType == "dat" || caseType == "давальний")
            {
                // Чоловічі імена на приголосний: Іван -> Івану
                if (IsConsonant(lastChar))
                    return word + "у";
                // Імена на -о: Петро -> Петру
                if (lastChar == 'о')
                    return word[..^1] + "у";
                // Імена на -а: Марія -> Марії
                if (lastChar == 'а')
                    return word[..^1] + "і";
                // Імена на -я: Софія -> Софії
                if (lastChar == 'я')
                    return word[..^1] + "ї";
                return word;
            }

            // Знахідний відмінок (кого? що?)
            if (caseType == "accusative" || caseType == "acc" || caseType == "знахідний")
            {
                // Чоловічі імена на приголосний: Іван -> Івана
                if (IsConsonant(lastChar))
                    return word + "а";
                // Імена на -о: Петро -> Петра
                if (lastChar == 'о')
                    return word[..^1] + "а";
                // Імена на -а: Марія -> Марію
                if (lastChar == 'а')
                    return word[..^1] + "ю";
                // Імена на -я: Софія -> Софію
                if (lastChar == 'я')
                    return word[..^1] + "ю";
                return word;
            }

            // Орудний відмінок (ким? чим?)
            if (caseType == "instrumental" || caseType == "ins" || caseType == "орудний")
            {
                // Чоловічі імена на приголосний: Іван -> Іваном
                if (IsConsonant(lastChar))
                    return word + "ом";
                // Імена на -о: Петро -> Петром
                if (lastChar == 'о')
                    return word[..^1] + "ом";
                // Імена на -а: Марія -> Марією
                if (lastChar == 'а')
                    return word[..^1] + "єю";
                // Імена на -я: Софія -> Софією
                if (lastChar == 'я')
                    return word[..^1] + "єю";
                return word;
            }

            // Місцевий відмінок (на кому? на чому?)
            if (caseType == "locative" || caseType == "loc" || caseType == "місцевий")
            {
                // Чоловічі імена на приголосний: Іван -> Іванові
                if (IsConsonant(lastChar))
                    return word + "ові";
                // Імена на -о: Петро -> Петрові
                if (lastChar == 'о')
                    return word[..^1] + "ові";
                // Імена на -а: Марія -> Марії
                if (lastChar == 'а')
                    return word[..^1] + "і";
                // Імена на -я: Софія -> Софії
                if (lastChar == 'я')
                    return word[..^1] + "ї";
                return word;
            }

            return word;
        }

        /// <summary>
        /// Перевіряє чи є символ приголосним
        /// </summary>
        private bool IsConsonant(char c)
        {
            var vowels = new[] { 'а', 'е', 'є', 'и', 'і', 'ї', 'о', 'у', 'ю', 'я' };
            return !vowels.Contains(char.ToLower(c));
        }

        /// <summary>
        /// SUBJECT - повертає ім'я у називному відмінку (хто?)
        /// Використання: SUBJECT($entity)
        /// </summary>
        private ILocValue FormatUkrainianSubject(LocArgs args)
        {
            var name = ((LocValueString)args.Args[0]).Value;
            return new LocValueString(name); // Називний відмінок - без змін
        }

        /// <summary>
        /// OBJECT - повертає ім'я у знахідному відмінку (кого? що?)
        /// Використання: OBJECT($entity)
        /// </summary>
        private ILocValue FormatUkrainianObject(LocArgs args)
        {
            var name = ((LocValueString)args.Args[0]).Value;
            return new LocValueString(DeclineUkrainianWord(name, "accusative"));
        }

        /// <summary>
        /// POSS-ADJ - присвійний прикметник (чий? чия? чиє?)
        /// Використання: POSS-ADJ($entity)
        /// Повертає: його/її/їхній
        /// </summary>
        private ILocValue FormatUkrainianPossessiveAdjective(LocArgs args)
        {
            var name = ((LocValueString)args.Args[0]).Value;
            // Для української мови використовуємо "його/її/їхній" залежно від роду
            // За замовчуванням повертаємо "його" (можна розширити з визначенням роду)
            return new LocValueString("його");
        }

        /// <summary>
        /// POSS-PRONOUN - присвійний займенник
        /// Використання: POSS-PRONOUN($entity)
        /// </summary>
        private ILocValue FormatUkrainianPossessivePronoun(LocArgs args)
        {
            var name = ((LocValueString)args.Args[0]).Value;
            return new LocValueString("його");
        }

        /// <summary>
        /// REFLEXIVE - зворотний займенник (себе, собі, собою)
        /// Використання: REFLEXIVE($entity)
        /// </summary>
        private ILocValue FormatUkrainianReflexive(LocArgs args)
        {
            var name = ((LocValueString)args.Args[0]).Value;
            return new LocValueString("себе");
        }

        /// <summary>
        /// CONJUGATE-BE - дієслово "бути" у правильній формі
        /// Використання: CONJUGATE-BE($entity)
        /// </summary>
        private ILocValue FormatUkrainianConjugateBe(LocArgs args)
        {
            var name = ((LocValueString)args.Args[0]).Value;
            // Для української мови повертаємо відповідну форму
            return new LocValueString("є");
        }

        /// <summary>
        /// CONJUGATE-BASIC - базове дієвідмінювання
        /// Використання: CONJUGATE-BASIC($entity, "singular", "plural")
        /// </summary>
        private ILocValue FormatUkrainianConjugateBasic(LocArgs args)
        {
            var name = ((LocValueString)args.Args[0]).Value;
            var singular = ((LocValueString)args.Args[1]).Value;
            var plural = args.Args.Length > 2 ? ((LocValueString)args.Args[2]).Value : singular;

            // За замовчуванням використовуємо форму однини
            return new LocValueString(singular);
        }

        /// <summary>
        /// PROPER - форматує ім'я як власне (з великої літери)
        /// Використання: PROPER($entity)
        /// </summary>
        private ILocValue FormatUkrainianProper(LocArgs args)
        {
            var name = ((LocValueString)args.Args[0]).Value;
            if (string.IsNullOrEmpty(name))
                return new LocValueString(name);

            return new LocValueString(char.ToUpper(name[0]) + name[1..]);
        }
    }
}
