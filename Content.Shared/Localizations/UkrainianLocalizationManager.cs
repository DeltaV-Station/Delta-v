using System.Globalization;
using Robust.Shared.IoC;
using Robust.Shared.Localization;

namespace Content.Shared.Localizations
{
    /// <summary>
    /// Розширення для української локалізації з підтримкою граматичних правил
    /// </summary>
    public sealed class UkrainianLocalizationManager
    {
        [Dependency] private readonly ILocalizationManager _loc = default!;

        private const string CultureUk = "uk-UA";

        public void Initialize()
        {
            var cultureUk = new CultureInfo(CultureUk);

            // Реєструємо українську культуру
            _loc.LoadCulture(cultureUk);

            // Додаємо українські функції
            _loc.AddFunction(cultureUk, "UKPLURAL", FormatUkrainianPlural);
            _loc.AddFunction(cultureUk, "UKGENDER", FormatUkrainianGender);
            _loc.AddFunction(cultureUk, "UKCASE", FormatUkrainianCase);
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
        /// Допоміжна функція для визначення форми множини числа
        /// </summary>
        public static string GetPluralForm(int count)
        {
            var lastDigit = Math.Abs(count) % 10;
            var lastTwoDigits = Math.Abs(count) % 100;

            if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
                return "many";

            if (lastDigit == 1)
                return "one";

            if (lastDigit >= 2 && lastDigit <= 4)
                return "few";

            return "many";
        }
    }
}
