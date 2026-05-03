using Content.Shared.Shuttles.Components;
using JetBrains.Annotations;
using Robust.Shared.Map.Components;

namespace Content.Shared.Shuttles.Systems;

/// <summary>
/// This is the DeltaV System for avoiding merge conflicts.
/// </summary>
public abstract partial class SharedShuttleSystem
{
    /// <summary>
    /// Checks if the GridUid has the specified IFF-Flag.
    /// </summary>
    /// <param name="gridUid">The grid whose IFF-Flags are being checked.</param>
    /// <param name="requiredFlag">The required IFF-Flag.</param>
    /// <param name="component">The IFF component of the grid.</param>
    /// <returns>
    /// Returns true if the grid has the required IFF-Flag, otherwise false.
    /// </returns>
    /// <remarks>
    /// Returns false if the Uid is not a grid and the required IFF-Flag is anything but <see cref="IFFFlags.None"/>, otherwise true.
    /// </remarks>
    [PublicAPI]
    public bool HasIFFFlag(EntityUid gridUid, IFFFlags requiredFlag, IFFComponent? component = null)
    {
        // Check if it's a grid.
        if (!HasComp<MapGridComponent>(gridUid))
            return requiredFlag == IFFFlags.None;

        component ??= EnsureComp<IFFComponent>(gridUid);

        return component.Flags.HasFlag(requiredFlag);
    }

}
