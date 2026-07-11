using Content.Shared._DV.CosmicCult.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.CosmicCult;

[Serializable, NetSerializable]
public enum CosmicGlyphDrawUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class CosmicGlyphDrawBuiState(List<ProtoId<GlyphPrototype>> glyphs) : BoundUserInterfaceState
{
    public List<ProtoId<GlyphPrototype>> Glyphs = glyphs;
}

[Serializable, NetSerializable]
public sealed class CosmicGlyphDrawSelectedMessage(ProtoId<GlyphPrototype> glyphProtoId) : BoundUserInterfaceMessage
{
    public ProtoId<GlyphPrototype> GlyphProtoId = glyphProtoId;
}
