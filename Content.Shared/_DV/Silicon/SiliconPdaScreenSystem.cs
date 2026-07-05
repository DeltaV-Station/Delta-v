using Content.Shared.Actions;
using Content.Shared.PDA;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;

namespace Content.Shared._DV.Silicon;

public sealed class SiliconPdaScreenSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PdaComponent, TogglePdaScreenEvent>(OnTogglePdaScreen);
    }

    private void OnTogglePdaScreen(Entity<PdaComponent> ent, ref TogglePdaScreenEvent args)
    {
        if (args.Handled || !TryComp<ActorComponent>(ent, out var actor))
            return;

        args.Handled = true;

        _userInterface.TryToggleUi(ent.Owner, PdaUiKey.Key, actor.PlayerSession);
    }
}

public sealed partial class TogglePdaScreenEvent : InstantActionEvent;
