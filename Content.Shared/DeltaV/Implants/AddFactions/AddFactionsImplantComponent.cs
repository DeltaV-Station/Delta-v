using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeltaV.Implants.AddFactions;

/// <summary>
///     Will add all the factions to the person being implanted.
/// </summary>
[RegisterComponent]
public sealed partial class AddFactionsImplantComponent : Component
{
    [DataField(required: true)]
    public HashSet<ProtoId<NpcFactionPrototype>> Factions;

    [DataField]
    public HashSet<ProtoId<NpcFactionPrototype>> AddedFactions = new();
}
