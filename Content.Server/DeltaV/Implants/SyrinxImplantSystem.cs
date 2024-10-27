using Content.Server.VoiceMask;
using Content.Shared.Implants;
using Content.Shared.Tag;

namespace Content.Server.Implants;

public sealed class SubdermalBionicSyrinxImplantSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;

    [ValidatePrototypeId<TagPrototype>]
    private const string BionicSyrinxImplant = "BionicSyrinxImplant";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoiceMaskComponent, ImplantImplantedEvent>(OnInsert);
    }

    private void OnInsert(EntityUid uid, VoiceMaskComponent component, ImplantImplantedEvent args)
    {
        if (!args.Implanted.HasValue ||
            !_tag.HasTag(args.Implant, BionicSyrinxImplant))
            return;

        // Update the name so it's the entities default name. You can't take it off like a voice mask so it's important!
        component.VoiceMaskName = Name(args.Implanted.Value);
    }
}
