/*
 * Delta-V - This file is licensed under AGPLv3
 * Copyright (c) 2024 Delta-V Contributors
 * See AGPLv3.txt for details.
*/

using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class CrimeAssistUiState : BoundUserInterfaceState
{
    public CrimeAssistUiState()
    { }
}

[Serializable, NetSerializable]
public sealed class CrimeAssistSyncMessageEvent : CartridgeMessageEvent
{
    public CrimeAssistSyncMessageEvent()
    { }
}
