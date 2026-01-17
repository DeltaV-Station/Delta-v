using Content.Server.Access.Systems;
using Content.Shared._DV.Access.Components;
using Content.Shared._DV.Access.Systems;
using Content.Shared._DV.NanoChat;
using Content.Shared.Implants.Components;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server._DV.Access.Systems;

/// <summary>
/// Server side handling subdermal ID cards.
/// </summary>
public sealed class SubdermalIdCardSystem : SharedSubdermalIdCardSystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly IdCardSystem _cardSystem = default!;
    [Dependency] private readonly SharedNanoChatSystem _nanochat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SubdermalIdCardComponent, EntGotInsertedIntoContainerMessage>(OnImplantInserted);
    }

    /// <summary>
    /// Handles when a subdermal ID impant is inserted into a container matching the correct ID.
    /// Will then spawn the relevant ID card and binding it to the owner of the implant.
    /// </summary>
    /// <param name="ent">Implant which was inserted into a container.</param>
    /// <param name="args">Args for the event, notably the container the implant was inserted into.</param>
    private void OnImplantInserted(Entity<SubdermalIdCardComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != ImplanterComponent.ImplantSlotId)
            return; // We weren't inserted into an implant container on the entity, do nothing.

        if (!TryComp<SubdermalImplantComponent>(ent, out var subdermalComponent) ||
            !subdermalComponent.ImplantedEntity.HasValue)
            return; // We require a subdermal implant inside an entity.

        if (!_container.TryGetContainer(ent, SubdermalIdCardComponent.IDCardContainerName, out var container))
            return; // No valid container for the IDCard to be stored in.

        var idCard = Spawn(ent.Comp.IdCardProto);
        if (!_container.Insert(idCard, container))
        {
            // Failed to insert the card into the container
            // Make sure we clean up dead ID cards if we weren't able to create/insert.
            QueueDel(idCard);
            return;
        }

        // Ensure subdermal IDCards do not get listed as nanochat targets
        _nanochat.SetListNumber(idCard, false);

        if (ent.Comp.UpdateName)
        {
            var metadata = MetaData(subdermalComponent.ImplantedEntity.Value);
            _cardSystem.TryChangeFullName(idCard, metadata.EntityName);
        }
    }
}
