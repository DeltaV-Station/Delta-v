using Content.Server.VoiceMask;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server.Implants;

public sealed class SubdermalBionicSyrinxImplantSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [ValidatePrototypeId<SpeciesPrototype>]
    public const string HarpySpeciesName = "species-name-harpy";

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
        if (!args.Implanted.HasValue ||
            !TryComp<HumanoidAppearanceComponent>(args.Implanted.Value, out var appearance))
        {
            return;
        }

        var speciesPrototype = _prototypeManager.Index(appearance.Species);
        var voicemask = EnsureComp<VoiceMaskComponent>(args.Implanted.Value);
        voicemask.VoiceName = MetaData(args.Implanted.Value).EntityName;
        voicemask.Enabled = speciesPrototype?.Name == HarpySpeciesName;
        Dirty(args.Implanted.Value, voicemask);
    }

    private void OnRemove(EntityUid uid, VoiceMaskerComponent component, EntGotRemovedFromContainerMessage args)
    {
        if (!TryComp<SubdermalImplantComponent>(uid, out var implanted) || implanted.ImplantedEntity == null)
            return;

        RemComp<VoiceMaskComponent>((EntityUid) implanted.ImplantedEntity);
    }
}
