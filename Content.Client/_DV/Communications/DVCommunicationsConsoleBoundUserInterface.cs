using Content.Shared._DV.Communications;
using Robust.Client.UserInterface;

namespace Content.Client._DV.Communications;

public sealed class DVCommunicationsConsoleBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entity = default!;

    private DVCommunicationsConsoleMenu? _menu;

    public DVCommunicationsConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<DVCommunicationsConsoleMenu>();
        _menu.OnMessage += SendMessage;
        if (_entity.TryGetComponent<DVCommunicationsConsoleComponent>(Owner, out var comp))
            Update((Owner, comp));
    }

    public void Update(Entity<DVCommunicationsConsoleComponent> ent)
    {
        _menu?.Update(ent);
    }
}
