namespace Content.Shared._DV.Access;

/// <summary>
///     Unlocks a LockComponent when a station's alert level is changed to one of the specified levels
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class UnlockOnAlertLevelComponent : Component
{
    [DataField(required: true)]
    public List<string> AlertLevels = new();
}
