using Content.Shared.UserInterface;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Client.Items;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

using Content.Shared._DV.MultipleUiKey;

namespace Content.Client._DV.MultipleUiKey;

public sealed partial class MultipleUiKeySystem : SharedMultipleUiKeySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.ItemStatus<MultipleUiKeyComponent>(ent => new MultipleUiKeyStatusControl(ent));
        return;
    }

    public override void SetUiKey(EntityUid uid, MultipleUiKeyComponent comp, EntityUid user)
    {
        base.SetUiKey(uid, comp, user);

        comp.LabelUpdateNeeded = true;

        // Show the mode change in a popup
        string modeString = Loc.GetString(comp.PopUpMessage, ("mode", Loc.GetString(comp.Modes[comp.Index].Name)));
        _popup.PopupClient(modeString, uid, user);

        return;
    }
}
