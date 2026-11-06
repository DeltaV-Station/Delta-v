using Content.Shared.Objectives;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Objectives.Eui;

/// <summary>
/// Bundle of data for objectives to display for editing.
/// </summary>
/// <param name="entity">The original objective entity.</param>
/// <param name="issuer">The issuer for the objective.</param>
/// <param name="proto">An optional prototype used to spawn the entity.</param>
/// <param name="info">All other ObjectiveInfo for the entity.</param>
[Serializable, NetSerializable]
public sealed class ObjectiveData(NetEntity entity, string issuer, EntProtoId? proto, ObjectiveInfo info)
{
    public NetEntity Entity = entity;
    public string Issuer = issuer;
    public EntProtoId? Proto = proto;
    public ObjectiveInfo Info = info;

    /// <summary>
    /// Provides a clone of the data.
    /// </summary>
    /// <returns>A full copy of the original data.</returns>
    public ObjectiveData Clone()
    {
        return (ObjectiveData)MemberwiseClone();
    }

    /// <summary>
    /// Copies data from another ObjectiveData.
    /// </summary>
    /// <param name="other">The other data to copy from.</param>
    public void CopyFrom(ObjectiveData other)
    {
        Entity = other.Entity;
        Issuer = other.Issuer;
        Proto = other.Proto;
        Info = other.Info;
    }
}
