using Content.Shared.PDA;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Verbs;

namespace Content.Shared._DV.Silicon;

public sealed class BorgPdaSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgPdaComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerbs);
    }

    private void OnGetAlternativeVerbs(Entity<BorgPdaComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanComplexInteract || !HasComp<StationAiHeldComponent>(args.User) || !args.CanInteract)
            return;

        var user = args.User;
        var target = args.Target;

        var verb = new AlternativeVerb
        {
            Text = Loc.GetString("toggle-borg-pda"),
            Act = () =>
            {
                _userInterface.TryToggleUi(target, PdaUiKey.Key, user);
            }
        };
        args.Verbs.Add(verb);
    }
}
