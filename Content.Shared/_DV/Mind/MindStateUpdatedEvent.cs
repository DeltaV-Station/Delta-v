namespace Content.Shared._DV.Mind;

using Content.Shared.Mind.Components;

[ByRefEvent]
public record struct MindStateUpdatedEvent(MindState State);
