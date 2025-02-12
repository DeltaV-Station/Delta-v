using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Actions.Events;

public sealed partial class CreateFoxfireActionEvent : InstantActionEvent
{
    // The foxfire prototype to use
    [DataField]
    public EntProtoId FoxfirePrototype = "Foxfire";

    [DataField]
    public EntProtoId FoxfireActionId = "FoxfireAction";

    public EntityUid? FoxfireAction;

}

public readonly record struct FoxfireDestroyedEvent;
