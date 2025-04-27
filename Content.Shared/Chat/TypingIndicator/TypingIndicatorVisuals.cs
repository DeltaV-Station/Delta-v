using Robust.Shared.Serialization;

namespace Content.Shared.Chat.TypingIndicator;

[Serializable, NetSerializable]
public enum TypingIndicatorVisuals : byte
{
    IsTyping,
    OverrideProto, // DeltaV: Indicator Override
}

[Serializable]
public enum TypingIndicatorLayers : byte
{
    Base
}
