/*
 * Delta-V - This file is licensed under AGPLv3
 * Copyright (c) 2024 Delta-V Contributors
 * See AGPLv3.txt for details.
*/

using Robust.Client.UserInterface;
using Content.Client.UserInterface.Fragments;
using Content.Shared.DeltaV.CartridgeLoader.Cartridges;
using Content.Shared.CartridgeLoader;
using Robust.Shared.Prototypes;

namespace Content.Client.DeltaV.CartridgeLoader.Cartridges;

public sealed partial class CrimeAssistUi : UIFragment
{
    private CrimeAssistUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new CrimeAssistUiFragment();

        _fragment.OnSync += _ => SendSyncMessage(userInterface);
    }

    private void SendSyncMessage(BoundUserInterface userInterface)
    {
        var syncMessage = new CrimeAssistSyncMessageEvent();
        var message = new CartridgeUiMessage(syncMessage);
        userInterface.SendMessage(message);
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
    }
}
