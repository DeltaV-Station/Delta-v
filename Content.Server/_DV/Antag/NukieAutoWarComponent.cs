namespace Content.Server._DV.Antag;

[RegisterComponent, Access(typeof(NukieOperationSystem)), AutoGenerateComponentPause]
public sealed partial class NukieAutoWarComponent : Component
{
    /// <summary>
    ///     Automatically calls war after set time.
    /// </summary>
    [DataField, AutoPausedField]
    public TimeSpan AutoWarCallTime;
}
