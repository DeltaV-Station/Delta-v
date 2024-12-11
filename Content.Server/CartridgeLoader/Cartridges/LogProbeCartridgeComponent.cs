﻿using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.DeltaV.CartridgeLoader.Cartridges; // DeltaV
using Robust.Shared.Audio;

namespace Content.Server.CartridgeLoader.Cartridges;

[RegisterComponent]
[Access(typeof(LogProbeCartridgeSystem))]
public sealed partial class LogProbeCartridgeComponent : Component
{
    /// <summary>
    /// The list of pulled access logs
    /// </summary>
    [DataField, ViewVariables]
    public List<PulledAccessLog> PulledAccessLogs = new();

    /// <summary>
    /// The sound to make when we scan something with access
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier SoundScan = new SoundPathSpecifier("/Audio/Machines/scan_finish.ogg");

    /// <summary>
    /// DeltaV: The last scanned NanoChat data, if any
    /// </summary>
    [DataField]
    public NanoChatData? ScannedNanoChatData;
}
