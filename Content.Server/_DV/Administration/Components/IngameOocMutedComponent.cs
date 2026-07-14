namespace Content.Server._DV.Administration.Components;

/// <summary>
/// This Component is attached to mind entities of players whose LOOC/Deadchat access should be restricted.
/// </summary>
[RegisterComponent]
public sealed partial class InGameOocMutedComponent : Component
{
    public bool MuteLooc;
    public bool MuteDeadchat;
}
