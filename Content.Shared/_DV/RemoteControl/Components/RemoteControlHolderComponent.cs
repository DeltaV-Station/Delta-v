using Content.Shared._DV.RemoteControl.EntitySystems;

namespace Content.Shared._DV.RemoteControl.Components;

/// <summary>
/// Marks that this entity currently has a remote control equipped and may give orders via pointing.
/// Since we're using pointing for this, we rely on this component to mark entities that need to have special
/// handling for their pointing.
/// </summary>
[RegisterComponent]
[Access(typeof(SharedRemoteControlSystem))]
public sealed partial class RemoteControlHolderComponent : Component
{
    /// <summary>
    /// The remote control that this holder has equipped on their body.
    /// </summary>
    public EntityUid Control;
}
