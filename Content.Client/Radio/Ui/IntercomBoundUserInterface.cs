using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client.Radio.Ui;

[UsedImplicitly]
public sealed class IntercomBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private IntercomMenu? _menu;

    public IntercomBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {

    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<IntercomMenu>();

        if (EntMan.TryGetComponent(Owner, out IntercomComponent? intercom))
        {
            // BEGIN DeltaV - Add microphone and speaker component parameters to Update
            EntMan.TryGetComponent(Owner, out RadioMicrophoneComponent? microphone);
            EntMan.TryGetComponent(Owner, out RadioSpeakerComponent? speaker);
            _menu.Update(Owner, intercom, microphone, speaker);
            // END DeltaV
        }

        _menu.OnMicPressed += enabled =>
        {
            SendMessage(new ToggleIntercomMicMessage(enabled));
        };
        _menu.OnSpeakerPressed += enabled =>
        {
            SendMessage(new ToggleIntercomSpeakerMessage(enabled));
        };
        _menu.OnChannelSelected += channel =>
        {
            SendMessage(new SelectIntercomChannelMessage(channel));
        };
    }

    public void Update(Entity<IntercomComponent> ent)
    {
        // BEGIN DeltaV - Add microphone and speaker component parameters to Update
        if (_menu == null)
            return;

        EntMan.TryGetComponent(Owner, out RadioMicrophoneComponent? microphone);
        EntMan.TryGetComponent(Owner, out RadioSpeakerComponent? speaker);
        _menu.Update(Owner, ent.Comp, microphone, speaker);
        // END DeltaV
    }
}
