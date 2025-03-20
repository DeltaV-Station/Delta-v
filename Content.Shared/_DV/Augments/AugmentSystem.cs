using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Content.Shared.Interaction;

namespace Content.Shared._DV.Augments;

public sealed class AugmentSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AugmentComponent, OrganAddedToBodyEvent>(OnOrganOrganAddedToBody);
        SubscribeLocalEvent<AugmentComponent, OrganRemovedFromBodyEvent>(OnOrganOrganRemovedFromBody);
        SubscribeLocalEvent<InstalledAugmentsComponent, AccessibleOverrideEvent>(OnAccessibleOverride);
    }

    private void OnOrganOrganAddedToBody(Entity<AugmentComponent> augment, ref OrganAddedToBodyEvent args)
    {
        var installed = EnsureComp<InstalledAugmentsComponent>(args.Body);
        installed.InstalledAugments.Add(GetNetEntity(augment));
    }

    private void OnOrganOrganRemovedFromBody(Entity<AugmentComponent> augment, ref OrganRemovedFromBodyEvent args)
    {
        if (!TryComp<InstalledAugmentsComponent>(args.OldBody, out var installed))
            return;

        installed.InstalledAugments.Remove(GetNetEntity(augment));
        if (installed.InstalledAugments.Count == 0)
            RemComp<InstalledAugmentsComponent>(args.OldBody);
    }

    private void OnAccessibleOverride(Entity<InstalledAugmentsComponent> augment, ref AccessibleOverrideEvent args)
    {
        if (!TryComp<OrganComponent>(args.Target, out var organ) || organ.Body != args.User)
            return;

        // let the user interact with their installed augments
        args.Handled = true;
        args.Accessible = true;
    }
}
