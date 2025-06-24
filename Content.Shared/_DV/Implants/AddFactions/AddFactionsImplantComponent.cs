using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Implants.AddFactions;

/// <summary>
///     Will add all the factions to the person being implanted.
/// </summary>
[RegisterComponent]
public sealed partial class AddFactionsImplantComponent : Component
{
    /// <summary>
    ///     These factions will be added when implanted.
    /// </summary>
    [DataField(required: true)]
    public HashSet<ProtoId<NpcFactionPrototype>> Factions;

    /// <summary>
    ///     These are the factions that were actually added. Used know what factions to remove when the implant is removed.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<NpcFactionPrototype>> AddedFactions = new();
}
