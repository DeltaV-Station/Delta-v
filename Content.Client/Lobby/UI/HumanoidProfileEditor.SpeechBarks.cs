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

        var sbmproto = _prototypeManager.EnumeratePrototypes<BarkPrototype>().ToList();
        var index = 0;
        var selected = 0;

        // When the BarkVoice value is null, eg not defaulted or selected by a player, finds the Species default
        var voiceChoice = Profile?.BarkVoice;
        if (voiceChoice == null && Profile?.Species != null)
        {
            foreach (var proto in sbmproto)
            {
                if (proto.SpeciesWhitelist != null && proto.SpeciesWhitelist.Contains(Profile.Species))
                {
                    voiceChoice = proto.ID;
                    break;
                }
            }
            voiceChoice ??= "Generic High";
        }

        // Species whitelisting limits what voices are available in character creation.
        foreach (var proto in sbmproto)
        {
            if (proto.SpeciesWhitelist != null && !proto.SpeciesWhitelist.Contains(Profile?.Species ?? ""))
                continue;

            if (voiceChoice == proto.ID)
                selected = index;

            BarkVoiceButton.AddItem(proto.ID);
            index++;
        }

        BarkVoiceButton.SelectId(selected);
    }

    private void OnBarkVoiceChanged(OptionButton.ItemSelectedEventArgs args)
    {
        BarkVoiceButton.SelectId(args.Id);
        var sbmproto = _prototypeManager.EnumeratePrototypes<BarkPrototype>()
            .Where(p => p.SpeciesWhitelist == null || p.SpeciesWhitelist.Contains(Profile?.Species ?? ""))
            .ToList();

        if (args.Id >= 0 && args.Id < sbmproto.Count)
        {
            SetProfile(Profile?.WithBarkVoice(sbmproto[args.Id].ID), CharacterSlot);
        }
    }

    /// <summary>
    /// For Previewing Speech Bark voice sounds
    /// </summary>
    private void OnBarkPreviewPressed()
    {
        var voice = Profile?.BarkVoice;
        if (voice == null)
            return;

        var ev = new PreviewBarkEvent { VoiceId = voice };
        IoCManager.Resolve<IEntitySystemManager>()
            .GetEntitySystem<Content.Client._DV.Speech.Barks.BarkSystem>()
            .PreviewVoice(ev);
    }
}
