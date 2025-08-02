using Content.Shared._starcup.AACTablet;
using Content.Shared.Chat;
using Content.Shared.Radio;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._DV.AACTablet.UI;

public sealed partial class AACWindow
{
    private string Prefix => (string?)RadioChannels.SelectedMetadata ?? SharedChatSystem.LocalPrefix.ToString();

    internal void Update(AACTabletBuiState msg)
    {
        RadioChannels.Clear();

        var id = 0;
        RadioChannels.AddItem("Local", id);
        RadioChannels.SetItemMetadata(RadioChannels.GetIdx(id), SharedChatSystem.LocalPrefix.ToString());

        foreach (var channel in msg.RadioChannels)
        {
            var channelProto = _prototype.Index<RadioChannelPrototype>(channel);
            RadioChannels.AddItem(channelProto.LocalizedName, ++id);
            RadioChannels.SetItemMetadata(RadioChannels.GetIdx(id), string.Concat(SharedChatSystem.RadioChannelPrefix, channelProto.KeyCode));
        }
    }

    private void OnChannelSelected(OptionButton.ItemSelectedEventArgs args)
    {
        RadioChannels.SelectId(args.Id);
    }
}
