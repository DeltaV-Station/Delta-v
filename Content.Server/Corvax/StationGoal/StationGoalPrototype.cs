using Robust.Shared.Prototypes;

namespace Content.Server.Corvax.StationGoal
{
    [Serializable, Prototype("stationGoal")]
    public sealed class StationGoalPrototype : IPrototype
    {
        [IdDataFieldAttribute] public string ID { get; } = default!;

        public string Text => Loc.GetString($"station-goal-{ID.ToLower()}");
    }
}
