using Robust.Shared.GameStates;

namespace Content.Shared._DV.NPC.Components
{
    /// Based on NPCRecentlyInjectedComponent
    /// Added when a medibot moves to inject someone
    /// So they don't get targeted again for at least a 30 seconds or until the medibot fails/completes the injection.
    [RegisterComponent, NetworkedComponent]
    public sealed partial class NPCIsInjectionTargetComponent : Component
    {
        /// <summary>
        /// The medibot in question
        /// </summary>
        [ViewVariables]
        public EntityUid? Medibot;

        [ViewVariables(VVAccess.ReadWrite), DataField("accumulator")]
        public float Accumulator = 0f;

        [ViewVariables(VVAccess.ReadWrite), DataField("removeTime")]
        public TimeSpan RemoveTime = TimeSpan.FromSeconds(30);
    }
}
