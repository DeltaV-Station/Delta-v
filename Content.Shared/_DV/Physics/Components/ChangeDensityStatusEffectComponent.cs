using Robust.Shared.GameStates;

namespace Content.Shared._DV.Physics.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChangeDensityStatusEffectComponent : Component
{
    /// <summary>
    /// The amount of density added by a new fixture.
    /// </summary>
    [DataField(required: true)]
    public int Amount;

    /// <summary>
    /// The ID of the fixture containing said density.
    /// </summary>
    [DataField(required: true)]
    public string FixtureId;
}
