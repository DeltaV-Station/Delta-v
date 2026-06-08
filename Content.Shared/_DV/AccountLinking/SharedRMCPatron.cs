using Robust.Shared.Serialization;

namespace Content.Shared._DV.AccountLinking;

[Serializable, NetSerializable]
public sealed class SharedPatron(string name, string tier)
{
    public readonly string Name = name;
    public readonly string Tier = tier;
}
