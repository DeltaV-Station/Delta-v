using Content.Shared.Implants;
//using Content.Shared.Storage;
//using Content.Shared.Storage.EntitySystems;
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
        SubscribeLocalEvent<RadioImplantComponent, EntGotRemovedFromContainerMessage>(OnRemoved);
    }

    /// <summary>
    /// Handles implantation of the implant.
    /// </summary>
    private void OnImplanted(Entity<RadioImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (args.Implanted is not {} user)
            return;

        ent.Comp.Implantee = user;
        Dirty(ent, ent.Comp);

        // make sure the person entity gets slapped with a component so it can react to it talking.
        var implanted = EnsureComp<HasRadioImplantComponent>(user);
        implanted.Implants.Add(ent);
        Dirty(user, implanted);
    }

    /// <summary>
    /// Handles removal of the implant from its containing mob.
    /// </summary>
    /// <remarks>Done via <see cref="EntGotRemovedFromContainerMessage"/> because there is no specific event for an implant being removed.</remarks>
    private void OnRemoved(Entity<RadioImplantComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (TerminatingOrDeleted(ent) ||
            ent.Comp.Implantee is not {} user ||
            // this gets fired if it gets removed from ANY container but really, we just want to know if it was removed from its owner...
            // so check if the ent we got implanted into matches the container's owner (here, the container's owner is the entity)
            user != args.Container.Owner ||
            !TryComp<HasRadioImplantComponent>(user, out var implanted))
            return;

        implanted.Implants.Remove(ent);
        if (implanted.Implants.Count == 0)
            RemComp<HasRadioImplantComponent>(user);
        else
            Dirty(user, implanted);
        ent.Comp.Implantee = null;
    }
}
