using Content.Server.DeltaV.Speech.Components;
using Content.Server.Speech;
using Content.Server.Speech.EntitySystems;
using System.Text.RegularExpressions;

namespace Content.Server.DeltaV.Speech.EntitySystems;

public sealed class CensoredAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CensoredAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    // converts left word when typed into the right word. For example typing you becomes ye.
    public string Accentuate(string message, CensoredAccentComponent component)
    {
        var msg = message;

        msg = _replacement.ApplyReplacements(msg, "censored");

        return msg;
    }

    private void OnAccentGet(EntityUid uid, CensoredAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
