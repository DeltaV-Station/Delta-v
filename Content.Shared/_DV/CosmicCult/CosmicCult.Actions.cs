using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.CosmicCult;

[RegisterComponent, NetworkedComponent]
public sealed class CosmicCultActionComponent : Component;
public sealed class EventCosmicSiphon : EntityTargetActionEvent;
public sealed class EventCosmicBlank : EntityTargetActionEvent;
public sealed class EventCosmicPlaceMonument : InstantActionEvent; //given to the cult leader on roundstart
public sealed class EventCosmicMoveMonument : InstantActionEvent; //given the the cult leader on hitting tier 2, taken away on hitting tier 3
public sealed class EventCosmicReturn : InstantActionEvent;
public sealed class EventCosmicLapse : EntityTargetActionEvent;
public sealed class EventCosmicGlare : InstantActionEvent;
public sealed class EventCosmicIngress : EntityTargetActionEvent;
public sealed class EventCosmicImposition : InstantActionEvent;
public sealed class EventCosmicNova : WorldTargetActionEvent;


// Rogue Ascended
public sealed class EventRogueCosmicNova : WorldTargetActionEvent;
public sealed class EventRogueInfection : EntityTargetActionEvent;
public sealed class EventRogueGrandShunt : InstantActionEvent;
public sealed class EventRogueSlumber : EntityTargetActionEvent;
