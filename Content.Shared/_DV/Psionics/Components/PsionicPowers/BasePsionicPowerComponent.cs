using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

/// <summary>
/// Entities with this component are psionically insulated from a source.
/// </summary>
public abstract partial class BasePsionicPowerComponent : Component
{
    /// <summary>
    /// The actual UID for the action entity. It'll be saved here when the component is initialized.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    /// <summary>
    /// The prototype ID for the action.
    /// It's set up in the YML and then referenced via a string here.
    /// </summary>
    [DataField]
    public virtual EntProtoId ActionProtoId { get; set; }

    /// <summary>
    /// The Loc string for the name of the power.
    /// </summary>
    [DataField]
    public virtual string PowerName { get; set; }

    /// <summary>
    /// The minimum glimmer amount that will be changed upon use of the psionic power.
    /// Should be lower than <see cref="MaxGlimmerChanged"/>.
    /// </summary>
    [DataField]
    public virtual int MinGlimmerChanged { get; set; }

    /// <summary>
    /// The maximum glimmer amount that will be changed upon use of the psionic power.
    /// Should be higher than <see cref="MinGlimmerChanged"/>.
    /// </summary>
    [DataField]
    public virtual int MaxGlimmerChanged { get; set; }
}
