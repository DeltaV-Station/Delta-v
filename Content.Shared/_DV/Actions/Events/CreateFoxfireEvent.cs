using Robust.Shared.Protoypes;

namespace Content.Shared.Actions.Events;

public sealed partial class CreateFoxfireActionEvent : InstantActionEvent
{
    # The foxfire prototype to use
    [DataField]
    public EntProtoId FoxfirePrototype = "Foxfire";

    [DateField]
    public EntProtoId FoxfireActionId = "FoxfireAction";

    public EntityUid? FoxfireAction;

}

public readonly record struct FoxfireDestroyedEvent;
