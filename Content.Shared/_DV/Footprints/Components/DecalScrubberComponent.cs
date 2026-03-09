using Content.Shared.DoAfter;
using Content.Shared.Fluids;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Footprints.Components;

[RegisterComponent]
public sealed partial class DecalScrubberComponent : Component
{

    [DataField]
    public float Radius = 1f;

    [DataField]
    public TimeSpan DoAfterLength = TimeSpan.FromSeconds(2);

    [DataField]
    public EntityCoordinates? LastClick;

    [DataField]
    public SoundSpecifier ScrubSound = AbsorbentComponent.DefaultTransferSound;
}

[Serializable, NetSerializable]
public sealed partial class DecalScrubberDoAfterEvent : SimpleDoAfterEvent;
