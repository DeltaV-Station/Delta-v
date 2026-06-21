using Content.Shared.Speech;
using Content.Server._DV.Speech.Components;
using Content.Server._DV.Speech.SpeechTranslation;

namespace Content.Server._DV.Speech.EntitySystems;

public sealed class LateralLispSystem : EntitySystem
{
    private readonly SpeechTranslationSystem _st = new();

    public override void Initialize()
    {
        base.Initialize();

        _st.AddRule("sh", "shl", position: MatchPosition.Anywhere);
        _st.AddRule("ch", "chl", position: MatchPosition.Anywhere);
        _st.AddRule("zh", "zhl", position: MatchPosition.Anywhere);
        _st.AddRule("s", "shl", position: MatchPosition.Anywhere);
        _st.AddRule("z", "zhl", position: MatchPosition.Anywhere);
        _st.AddRule("j", "zhl", position: MatchPosition.Anywhere);
        _st.AddRule("x", "kshl", position: MatchPosition.Anywhere);

        SubscribeLocalEvent<LateralLispComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, LateralLispComponent component, AccentGetEvent args)
    {
        args.Message = _st.Process(args.Message);
    }
}