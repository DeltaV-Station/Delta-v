using System.Diagnostics.CodeAnalysis;
using Content.Shared._DV.Access.Components;
using Content.Shared.Access.Components;
using Content.Shared.Implants.Components;
using Robust.Shared.Containers;

namespace Content.Shared._DV.Access.Systems;

/// <summary>
/// Shared handling for subdermal ID Cards.
/// </summary>
public abstract class SharedSubdermalIdCardSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public const string SubdermalContainerId = "subdermalIdCard";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SubdermalIdCardComponent, ContainerGettingRemovedAttemptEvent>(OnRemoveAttempt);
    }

    /// <summary>
    /// Attempts to find an ID card from within the subdermal card container, if one exists.
    /// </summary>
    /// <param name="ent">Entity to search.</param>
    /// <param name="idEntity">Variable to place the found entityuid into.</param>
    /// <returns>True if matching ID card entity was found, otherwise false.</returns>
    public bool TryGetIdCard(EntityUid ent, [NotNullWhen(true)] out EntityUid? idEntity)
    {
        idEntity = default;
        if (!_container.TryGetContainer(ent, ImplanterComponent.ImplantSlotId, out var implantContainer))
            return false; // This entity is not implanted with anything

        foreach (var implant in implantContainer.ContainedEntities)
        {
            if (!_container.TryGetContainer(implant, SubdermalContainerId, out var con) ||
                con.ContainedEntities.Count == 0)
                continue;

            foreach (var item in con.ContainedEntities)
            {
                if (!HasComp<IdCardComponent>(item))
                    continue; // Not an ID card

                idEntity = item;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Handles when a subdermal ID card implant is attempted to be removed, blocking it completely.
    /// </summary>
    /// <param name="ent">The entity which is being removed.</param>
    /// <param name="args">Args for the event.</param>
    private void OnRemoveAttempt(Entity<SubdermalIdCardComponent> ent, ref ContainerGettingRemovedAttemptEvent args)
    {
        // Subdermal ID cards are NEVER removable by any means.
        args.Cancel();
    }
}
