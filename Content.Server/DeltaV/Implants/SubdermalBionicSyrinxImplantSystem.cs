using Content.Server.VoiceMask;
using Content.Shared.Actions;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Tag;
using Robust.Shared.Containers;
using Robust.Shared.Network;

namespace Content.Server.Implants;

public sealed class SubdermalBionicSyrinxImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoiceMaskerComponent, ImplantImplantedEvent>(OnInsert);
        // We need to remove the VoiceMaskComponent from the owner before the implant is removed,
        // so we need to execute before the SubdermalImplantSystem.
        SubscribeLocalEvent<VoiceMaskerComponent, EntGotRemovedFromContainerMessage>(OnRemove, before: new[] { typeof(SubdermalImplantSystem) });
    }

    private void OnInsert(EntityUid uid, VoiceMaskerComponent component, ImplantImplantedEvent args)
    {
        if (!args.Implanted.HasValue)
            return;

        var voicemask = EnsureComp<VoiceMaskComponent>(args.Implanted.Value);
        voicemask.VoiceName = MetaData(args.Implanted.Value).EntityName;
    }

    private void OnRemove(EntityUid uid, VoiceMaskerComponent component, EntGotRemovedFromContainerMessage args)
    {
        if (!TryComp<SubdermalImplantComponent>(uid, out var implanted) || implanted.ImplantedEntity == null)
            return;

        RemComp<VoiceMaskComponent>((EntityUid) implanted.ImplantedEntity);
    }
}
