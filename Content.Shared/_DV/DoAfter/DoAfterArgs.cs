using Content.Shared.Mobs;

namespace Content.Shared.DoAfter;

public sealed partial class DoAfterArgs
{
    /// <summary>
    ///     Whether entering critical state will cancel the DoAfter.
    /// </summary>
    [DataField]
    public List<MobState>? BreakOnMobState;
}
