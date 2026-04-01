using Content.Client.Eui;
using JetBrains.Annotations;
using Robust.Client.Graphics;

using Content.Shared._Goobstation.Silicon.AiEarlyLeave;

namespace Content.Client._Goobstation.Silicon.AiEarlyLeave;

[UsedImplicitly]
public sealed class StationAiEarlyLeaveEui : BaseEui
{
    private readonly StationAiEarlyLeaveMenu _menu;
    private bool _sentResponse;

    public StationAiEarlyLeaveEui()
    {
        _menu = new StationAiEarlyLeaveMenu();

        _menu.DenyButton.OnPressed += _ =>
        {
            SendResponse(false);
            _menu.Close();
        };

        _menu.ConfirmButton.OnPressed += _ =>
        {
            SendResponse(true);
            _menu.Close();
        };
    }

    private void SendResponse(bool confirmed)
    {
        if (_sentResponse)
            return;

        _sentResponse = true;
        SendMessage(new StationAiEarlyLeaveMessage(confirmed));
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _menu.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();

        SendResponse(false);
        _menu.Close();
    }

}
