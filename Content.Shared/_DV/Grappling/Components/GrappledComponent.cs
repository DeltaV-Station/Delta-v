using Content.Shared._DV.Grappling.EntitySystems;
using Content.Shared.Alert;
using Content.Shared.DoAfter;
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
    [DataField]
    public EntityUid Grappler = EntityUid.Invalid;

    /// <summary>
    /// How much time is required to escape.
    /// </summary>
    [DataField]
    public TimeSpan EscapeTime = TimeSpan.FromSeconds(15);

    /// <summary>
    /// The in-progress DoAfter, if any.
    /// Used to cancel the doAfter if the grappler manually releases their victim.
    /// </summary>
    [DataField]
    public DoAfterId? DoAfterId = null;

    /// <summary>
    /// A list of all hands, if any, that have been disabled as part of the grapple
    /// via a virtual item.
    /// </summary>
    [DataField]
    public List<string> DisabledHands = new();
}

/// <summary>
/// Raised when a player manually clicks the grappled icon to begin attempting to escape.
/// </summary>
public sealed partial class EscapeGrappleAlertEvent : BaseAlertEvent;
