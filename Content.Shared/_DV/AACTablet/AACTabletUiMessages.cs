using Content.Shared._DV.QuickPhrase;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.AACTablet;

[Serializable, NetSerializable]
public enum AACTabletKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class AACTabletSendPhraseMessage(List<ProtoId<QuickPhrasePrototype>> phraseIds, string prefix) : BoundUserInterfaceMessage
{
    public List<ProtoId<QuickPhrasePrototype>> PhraseIds = phraseIds;
    public string Prefix = prefix; // starcup
}

// starcup
[Serializable, NetSerializable]
public sealed class AACTabletBuiState(HashSet<string> radioChannels) : BoundUserInterfaceState
{
    public HashSet<string> RadioChannels = radioChannels;
}
