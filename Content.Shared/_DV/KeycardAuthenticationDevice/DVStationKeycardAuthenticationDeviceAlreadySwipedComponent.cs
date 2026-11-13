using Robust.Shared.GameStates;

namespace Content.Shared._DV.KeycardAuthenticationDevice;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedDVStationKeycardAuthenticationDeviceSystem))]
public sealed partial class DVStationKeycardAuthenticationDeviceAlreadySwipedComponent : Component;
