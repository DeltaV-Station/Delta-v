using Robust.Shared.Serialization;

namespace Content.Shared._ES.Core.Range;

/// <summary>
/// Defines a integer range, inclusive on both ends.
/// </summary>
[DataDefinition, Serializable, NetSerializable]
public partial struct ESIntRange
{
    [DataField] public int Min;
    [DataField] public int Max;

    public bool Contains(int value)
    {
        return value >= Min && value <= Max;
    }

    public ESIntRange(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public ESIntRange(int value)
    {
        Min = value;
        Max = value;
    }
}
