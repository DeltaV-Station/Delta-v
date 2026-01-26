using System.Numerics;
using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Footprints.Components;

[RegisterComponent]
public sealed partial class DecalScrubberComponent : Component
{

    [DataField]
    public float FailureChance = 0.15f;

    [DataField]
    public float DecalDistance = 2f;

    [DataField]
    public TimeSpan DoAfterLength = TimeSpan.FromSeconds(1);

    [DataField]
    public EntityCoordinates? LastClick;
}

[Serializable, NetSerializable]
public sealed partial class DecalScrubberDoAfterEvent : SimpleDoAfterEvent;


// [Serializable, NetSerializable]
// public sealed partial class DecalScrubberTryUseEvent : EntityEventArgs
// {
//     public Vector2 ClickPosition;
//     public EntityUid ClickGrid;
//
// };
