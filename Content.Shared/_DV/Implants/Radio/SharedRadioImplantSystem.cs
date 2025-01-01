using Content.Shared.Actions;
using Content.Shared.Implants;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Containers;

namespace Content.Shared._DV.Implants.Radio;

/// <summary>
/// This handles radio implants, which you can implant to get access to a radio channel.
/// </summary>
public abstract class SharedRadioImplantSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<RadioImplantComponent, ImplantImplantedEvent>(OnImplanted);
        SubscribeLocalEvent<RadioImplantComponent, EntGotRemovedFromContainerMessage>(OnPossiblyUnimplanted);
    }

    /// <summary>
    /// Handles implantation of the implant.
    /// </summary>
    private void OnImplanted(EntityUid uid, RadioImplantComponent component, ImplantImplantedEvent args)
    {
        if (args.Implanted is not { Valid: true })
            return;

        component.Implantee = args.Implanted.Value;
        Dirty(uid, component);

        // make sure the person entity gets slapped with a component so it can react to it talking.
        var hasRadioImplantComponent = EnsureComp<HasRadioImplantComponent>(args.Implanted.Value);
        hasRadioImplantComponent.Implant = uid;
        Dirty(component.Implantee.Value, hasRadioImplantComponent);
    }


    /// <summary>
    /// Handles removal of the implant from its containing mob.
    /// </summary>
    /// <remarks>Done via <see cref="EntGotRemovedFromContainerMessage"/> because there is no specific event for an implant being removed.</remarks>
    private void OnPossiblyUnimplanted(EntityUid uid, RadioImplantComponent component, EntGotRemovedFromContainerMessage args)
    {
        if (Terminating(uid))
            return;

        // this gets fired if it gets removed from ANY container but really, we just want to know if it was removed from its owner...
        // so check if the ent we got implanted into matches the container's owner (here, the container's owner is the entity)
        if (component.Implantee is not null && component.Implantee == args.Container.Owner)
        {
            RemComp<HasRadioImplantComponent>(component.Implantee.Value);
            component.Implantee = null;
        }
    }
}
