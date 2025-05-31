using Content.Shared.Whitelist;
using Robust.Shared.Analyzers;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.SmartFridge;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class SmartFridgeComponent : Component
{
    [DataField]
    public string Container = "smart_fridge_inventory";

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist;

    [DataField]
    public SoundSpecifier? InsertSound = new SoundPathSpecifier("/Audio/Weapons/Guns/MagIn/revolver_magin.ogg");

    [DataField, AutoNetworkedField]
    public List<SmartFridgeEntry> Entries = new();

    [DataField, AutoNetworkedField]
    public Dictionary<SmartFridgeEntry, List<NetEntity>> ContainedEntries = new();

    [DataField]
    public TimeSpan EjectCooldown = TimeSpan.FromSeconds(1.2);

    [ViewVariables]
    public bool Ejecting => EjectEnd != null;

    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? EjectEnd;

    /// <summary>
    ///     Sound that plays when ejecting an item
    /// </summary>
    [DataField]
    public SoundSpecifier SoundVend = new SoundPathSpecifier("/Audio/Machines/machine_vend.ogg")
    {
        Params = new AudioParams
        {
            Volume = -4f,
            Variation = 0.15f
        }
    };

    /// <summary>
    ///     Sound that plays when an item can't be ejected
    /// </summary>
    [DataField]
    public SoundSpecifier SoundDeny = new SoundPathSpecifier("/Audio/Machines/custom_deny.ogg");
}

[Serializable, NetSerializable, DataRecord]
public record struct SmartFridgeEntry
{
    public string Name;

    public SmartFridgeEntry(string name)
    {
        Name = name;
    }
}

[Serializable, NetSerializable]
public enum SmartFridgeUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class SmartFridgeDispenseItemMessage(SmartFridgeEntry entry) : BoundUserInterfaceMessage
{
    public SmartFridgeEntry Entry = entry;
}

[Serializable, NetSerializable]
public sealed class SmartFridgeRemoveEntryMessage(SmartFridgeEntry entry) : BoundUserInterfaceMessage
{
    public SmartFridgeEntry Entry = entry;
}
