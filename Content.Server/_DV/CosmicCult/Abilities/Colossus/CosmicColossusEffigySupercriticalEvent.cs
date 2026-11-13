using Content.Server._DV.CosmicCult.Components;

namespace Content.Server._DV.CosmicCult.Abilities.Colossus;

[ByRefEvent]
public readonly record struct CosmicColossusEffigySupercriticalEvent(
    Entity<CosmicEffigyComponent> Effigy
);
