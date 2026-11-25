using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._ES.Sparks.Components;

/// <summary>
/// An entity with <see cref="ItemToggleComponent"/> that sparks when toggled
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(ESSparksSystem))]
public sealed partial class ESSparkOnItemToggleComponent : ESBaseSparkConfigurationComponent
{
    /// <summary>
    /// If true, sparks will occur when the item is toggled ON.
    /// If false, sparks will occur when the item is toggled OFF.
    /// </summary>
    /// <remarks>
    /// There is no third option.
    /// </remarks>
    [DataField]
    public bool ActivatedSpark = true;
}
