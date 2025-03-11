using Robust.Shared.Serialization;

namespace Content.Shared._DV.Traitor;

/// <summary>
/// This entity is being held ransom and can be purchased to teleport to the ATS.
/// </summary>
[RegisterComponent, Access(typeof(RansomSystem))]
public sealed partial class RansomComponent : Component
{
    [DataField]
    public int Ransom;
}

/// <summary>
/// Ransom data for an entity visible on a cargo request console.
/// </summary>
[Serializable, NetSerializable]
public readonly record struct RansomData(NetEntity Entity, string Name, int Price);

/// <summary>
/// BUI message for a cargo request console to purchase a ransomed entity.
/// It gets teleported to the ATS if successful.
/// </summary>
[Serializable, NetSerializable]
public sealed class RansomPurchaseMessage(NetEntity entity) : BoundUserInterfaceMessage
{
    public readonly NetEntity Entity = entity;
}
