using System.Text.RegularExpressions;
using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems;

public sealed class NoContractionsAccentComponentAccentSystem : EntitySystem
{
    private static readonly Regex RegexLowerS = new("s+");
    private static readonly Regex RegexUpperS = new("S+");
    private static readonly Regex RegexInternalX = new(@"(\w)x");
    private static readonly Regex RegexLowerEndX = new(@"\bx([\-|r|R]|\b)");
    private static readonly Regex RegexUpperEndX = new(@"\bX([\-|r|R]|\b)");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NoContractionsAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, NoContractionsAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        args.Message = message;
    }
}
