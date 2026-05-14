using Content.Shared.DoAfter;
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
    /// It's set up in the component, optionally overriden via YML and then referenced via a string here.
    /// </summary>
    [DataField]
    public abstract EntProtoId ActionProtoId { get; set; }

    /// <summary>
    /// The Loc string for the name of the power.
    /// </summary>
    [DataField]
    public abstract string PowerName { get; set; }

    /// <summary>
    /// The minimum glimmer amount that will be changed upon use of the psionic power.
    /// Should be lower than <see cref="MaxGlimmerChanged"/>.
    /// </summary>
    [DataField]
    public abstract int MinGlimmerChanged { get; set; }

    /// <summary>
    /// The maximum glimmer amount that will be changed upon use of the psionic power.
    /// Should be higher than <see cref="MinGlimmerChanged"/>.
    /// </summary>
    [DataField]
    public abstract int MaxGlimmerChanged { get; set; }

    /// <summary>
    /// Whether this ability can be removed via mindbreaking.
    /// </summary>
    /// <example>Revenants shouldn't be able to lose their powers.</example>
    [DataField]
    public bool CanBeRemoved = true;

    /// <summary>
    /// When a power uses a DoAfter, the ID can be saved here for convenience.
    /// It'll handle being dispelled automatically.
    /// It'll need to be broken up into the DoAfter EntityUid and ushort index first.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? DoAfterEntityId;

    /// <summary>
    /// When a power uses a DoAfter, the index can be saved here for convenience.
    /// It'll handle being dispelled automatically.
    /// It'll need to be broken up into the DoAfter EntityUid and ushort index first.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ushort? DoAfterIdIndex;

    /// <summary>
    /// Helper method to save a DoAfterId as DoAfterIds are not serializable and therefore cannot be networked.
    /// It's parts can be though, and can be rebuilt.
    /// </summary>
    /// <param name="doAfterId">The DoAfterId to save. If null, it'll remove the saved DoAfterId.</param>
    public void SaveDoAfterId(DoAfterId doAfterId)
    {
        DoAfterEntityId = doAfterId.Uid;
        DoAfterIdIndex = doAfterId.Index;
    }

    /// <summary>
    /// Helper method to remove the saved DoAfterId.
    /// </summary>
    public void RemoveSavedDoAfterId()
    {
        DoAfterEntityId = null;
        DoAfterIdIndex = null;
    }

    /// <summary>
    /// A helper method to get a saved DoAfterId.
    /// </summary>
    /// <returns>Returns a DoAfterId if one is present, null if not.</returns>
    public DoAfterId? GetDoAfterId()
    {
        if (DoAfterEntityId is not { } doAfterId
            || DoAfterIdIndex is not { } doAfterIndex)
            return null;

        return new DoAfterId(doAfterId, doAfterIndex);
    }
}
