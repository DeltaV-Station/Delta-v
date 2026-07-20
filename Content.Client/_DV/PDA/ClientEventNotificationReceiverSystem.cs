using Content.Client.PDA;
using Content.Shared._DV.Pager;
using Content.Shared.PDA;

namespace Content.Client._DV.PDA;

public sealed class ClientEventNotificationReceiverSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PagerComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);
    }

    private void OnAfterAutoHandleState(Entity<PagerComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!_userInterface.TryGetOpenUi<PdaBoundUserInterface>(ent.Owner, PdaUiKey.Key, out var bui))
            return;

        bui.UpdateLinkedDevices();
    }
}
