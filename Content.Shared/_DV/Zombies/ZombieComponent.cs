using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Zombies;

public sealed partial class ZombieComponent
{

    /// <summary>
    /// DeltaV - The list of factions that an entity is part of from before they were zombified.
    /// The zombification process removes them, but cloning a zombie copies the NPCFactionMemberComponent
    /// so they are still considered a zombie faction-wise, so we need to keep track of what they had right
    /// before the factions are removed.
    /// </summary>
    [DataField]
    public List<ProtoId<NpcFactionPrototype>> FactionsBeforeZombification = new();

    /// <summary>
    /// DeltaV - Tracks if the entity was a pacifist before turning into a zombie. Although, there are other
    /// scenarios where the person might be pacified but those are usually temporary.
    /// </summary>
    public bool WasPacisfistBeforeZombification = false;
}
