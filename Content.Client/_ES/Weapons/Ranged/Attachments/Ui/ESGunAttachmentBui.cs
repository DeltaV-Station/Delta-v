using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._ES.Weapons.Ranged.Attachments.Ui;

[UsedImplicitly]
public sealed class ESGunAttachmentBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private ESGunAttachmentWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<ESGunAttachmentWindow>();
        _window.Update(Owner);
    }

    public override void Update()
    {
        base.Update();

        _window?.Update(Owner);
    }
}
