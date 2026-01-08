using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;

namespace Content.Server.Speech.EntitySystems;

/// <summary>
/// System that replaces certain vocabulary to be similar to Anglish.
/// </summary>
public sealed class AnglishAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    private static readonly Regex RegexThLowercase = new(@"th");
    private static readonly Regex RegexThUppercaseShort = new(@"Th");
    private static readonly Regex RegexThUppercaseLong = new(@"TH");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnglishAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    public string Accentuate(string message, AnglishAccentComponent component)
    {
        var msg = message;

        msg = _replacement.ApplyReplacements(msg, "anglish");

        // replaces th with an "eth" at the start of words.
        msg = RegexThLowercase.Replace(msg, "ð");
        msg = RegexThUppercaseShort.Replace(msg, "Ð");
        msg = RegexThUppercaseLong.Replace(msg, "Ð");

        return msg;
    }

    private void OnAccentGet(EntityUid uid, AnglishAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
