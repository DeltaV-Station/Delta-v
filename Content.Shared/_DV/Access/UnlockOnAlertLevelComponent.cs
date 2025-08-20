namespace Content.Shared._DV.Access;

/// <summary>
///     Unlocks a LockComponent when the station's alert level is changed to one of the specified levels
/// </summary>
[RegisterComponent]
public sealed partial class UnlockOnAlertLevelComponent : Component
{
    [DataField]
    public List<string> AlertLevels = new();
}
