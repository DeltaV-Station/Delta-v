using Content.Shared._DV.Grappling.EntitySystems;
using Content.Shared.Alert;
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Grappling.Components;

/// <summary>
/// Marks this entity as having been grappled.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedGrapplingSystem))]
public sealed partial class GrappledComponent : Component
{
    /// <summary>
    /// The entity which is performing the grapple.
    /// </summary>
    [ViewVariables]
    public EntityUid Grappler = EntityUid.Invalid;

    /// <summary>
    /// How much time is required to escape.
    /// </summary>
    [ViewVariables]
    public TimeSpan EscapeTime = TimeSpan.FromSeconds(15);

    /// <summary>
    /// A list of any hands, if any, that have been disabled by the grappler.
    /// </summary>
    [ViewVariables]
    public List<DisabledHand>? DisabledHands = null;

    /// <summary>
    /// The in-progress DoAfter, if any.
    /// Used to cancel the doAfter if the grappler manually releases their victim.
    /// </summary>
    [ViewVariables]
    public DoAfterId? DoAfterId = null;
}

/// <summary>
/// Simple record covering information on hands disabled by the grappler.
/// </summary>
/// <param name="Name">Name of the hand.</param>
/// <param name="Location">Location of the hand.</param>
public sealed record DisabledHand(string Name, HandLocation Location);

/// <summary>
/// Raised when a player manually clicks the grappled icon to begin attempting to escape.
/// </summary>
public sealed partial class EscapeGrappleAlertEvent : BaseAlertEvent;
