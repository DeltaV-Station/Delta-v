using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Uprising;

[Serializable, NetSerializable]
public sealed partial class UprisingDisarmDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class UprisingArmDoAfter : SimpleDoAfterEvent;
