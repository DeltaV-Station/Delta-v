using Content.Shared._DV.Speech.Barks;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Client.Lobby.UI;

// Delta-V Speech Barks - Separated CS from the main to prevent upstream updates from colliding with the feature
// This is for a menu outlet to select the voices in the character editor.

public sealed partial class HumanoidProfileEditor
{
    private void UpdateBarkVoiceSelector()
    {
        BarkVoiceButton.Clear();

        var sbmproto = _prototypeManager.EnumeratePrototypes<BarkPrototype>();
        var index = 0;
        var selected = 0;

        foreach (var proto in sbmproto)
        {
            if (Profile?.BarkVoice == proto.ID)
                selected = index;

            BarkVoiceButton.AddItem(proto.ID);
            index++;
        }

        BarkVoiceButton.SelectId(selected);
    }

    private void OnBarkVoiceChanged(OptionButton.ItemSelectedEventArgs args)
    {
        BarkVoiceButton.SelectId(args.Id);
        var sbmproto = _prototypeManager.EnumeratePrototypes<BarkPrototype>().ToList();
        if (args.Id >= 0 && args.Id < sbmproto.Count)
        {
            SetProfile(Profile?.WithBarkVoice(sbmproto[args.Id].ID), CharacterSlot);
        }
    }
}
