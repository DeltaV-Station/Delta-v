using Content.Shared.Chemistry.Components;

namespace Content.Server.Chemistry.Components
{
    [RegisterComponent]
    public sealed partial class SolutionRegenerationSwitcherComponent : Component
    {
        [DataField("options", required: true), ViewVariables(VVAccess.ReadWrite)]
        public List<Solution> Options = default!;

        [DataField("currentIndex"), ViewVariables(VVAccess.ReadWrite)]
        public int CurrentIndex = 0;

        /// <summary>
        /// Should the already generated solution be kept when switching?
        /// </summary>
        [DataField("keepSolution"), ViewVariables(VVAccess.ReadWrite)]
        public bool KeepSolution = false;
    }
}
