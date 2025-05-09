using Content.Shared.Body.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Body.Components;

/// <summary>
/// Component for bodyparts and organs.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedBodySystem))]
[AutoGenerateComponentState]
public sealed partial class BodyMechanismComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Body;

    [DataField, AutoNetworkedField]
    public bool Enabled = true;
}
