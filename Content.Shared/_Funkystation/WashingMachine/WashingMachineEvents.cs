namespace Content.Shared._Funkystation.WashingMachine;

public sealed class WashingMachineIsBeingWashed : EntityEventArgs
{
    public EntityUid WashingMachine;
    public HashSet<EntityUid> Items;

    public WashingMachineIsBeingWashed(EntityUid washingMachine, HashSet<EntityUid> items)
    {
        WashingMachine = washingMachine;
        Items = items;
    }
}

public sealed class WashingMachineStartedWashingEvent : EntityEventArgs
{
    public HashSet<EntityUid> Items;

    public WashingMachineStartedWashingEvent(HashSet<EntityUid> items)
    {
        Items = items;
    }
}

public sealed class WashingMachineWashedEvent : EntityEventArgs
{
    public EntityUid WashingMachine;
    public HashSet<EntityUid> Items;

    public WashingMachineWashedEvent(EntityUid washingMachine, HashSet<EntityUid> items)
    {
        WashingMachine = washingMachine;
        Items = items;
    }
}

public sealed class WashingMachineFinishedWashingEvent : EntityEventArgs
{
    public HashSet<EntityUid> Items;

    public WashingMachineFinishedWashingEvent(HashSet<EntityUid> items)
    {
        Items = items;
    }
}
