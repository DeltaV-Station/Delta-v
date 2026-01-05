using Content.Shared._EE.FootPrint.Systems;
using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._EE.FootPrint;

/// <summary>
/// Component attached to individual footprint entities spawned on the ground.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(FootPrintsSystem))]
public sealed partial class FootPrintComponent : Component
{
    /// <summary>
    /// The entity that created this footprint (must have <see cref="FootPrintsComponent"/>).
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid PrintOwner;

    /// <summary>
    /// Name of the solution container for reagent residue left in the footprint.
    /// </summary>
    [DataField]
    public string SolutionName = "step";

    /// <summary>
    /// The solution container for this footprint's reagent residue.
    /// </summary>
    [ViewVariables]
    public Entity<SolutionComponent>? Solution;
}
