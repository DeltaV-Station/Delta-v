using Robust.Shared.Prototypes;

namespace Content.Server.Corvax.StationGoal
{
    [Serializable, Prototype("stationGoal")]
    public sealed class StationGoalPrototype : IPrototype
    {
        [IdDataFieldAttribute] public string ID { get; } = default!;

        public string Text => Loc.GetString($"station-goal-{ID.ToLower()}");

        [DataField]
        public int? MinPlayers = 0;
        [DataField]
        public int? MaxPlayers = 999;
        /// <summary>
        /// Goal may require certain items to complete. These items will appear near the receving fax machine at the start of the round.
        /// TODO: They should be spun up at the tradepost instead of at the fax machine, but I'm too lazy to do that right now. Maybe in the future.
        /// </summary>
        [DataField]
        public List<EntProtoId> Spawns = new();
    }
}
