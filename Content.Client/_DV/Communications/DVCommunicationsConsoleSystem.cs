using Content.Shared._DV.Communications;
using Robust.Client.GameObjects;

namespace Content.Client._DV.Communications;

public sealed class DVCommunicationsConsoleSystem : SharedDVCommunicationsConsoleSystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DVCommunicationsConsoleComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    private void OnHandleState(Entity<DVCommunicationsConsoleComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!_userInterface.TryGetOpenUi<DVCommunicationsConsoleBoundUserInterface>(ent.Owner,
                DVCommunicationsConsoleUi.Key,
                out var bui))
            return;

        bui.Update(ent);
    }
}
