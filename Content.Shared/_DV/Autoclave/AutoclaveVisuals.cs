using Robust.Shared.Serialization;

namespace Content.Shared._DV.Autoclave;

[Serializable, NetSerializable]
public enum AutoclaveVisuals : byte
{
    ProcessingLayer,
    IdleLayer,
    IsProcessing,
    IsIdle
}
