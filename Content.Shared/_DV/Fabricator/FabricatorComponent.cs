using Content.Shared.Item;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Fabricator;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedFabricatorSystem))]
public sealed partial class FabricatorComponent : Component
{
    [DataField]
    public float TimeMultiplier = 1.0f;

    [DataField]
    public List<ProtoId<FabricatorRecipeGroupPrototype>> RecipeGroups = new();

    [DataField]
    public ProtoId<ItemSizePrototype> MaxItemSize = "Normal";

    [DataField]
    public int MaxItems = 10;

    [DataField]
    public int MaxPerBatch = 10;

    [DataField]
    public SoundSpecifier StartSound = new SoundPathSpecifier("/Audio/Machines/microwave_start_beep.ogg");

    [DataField]
    public SoundSpecifier? DoneSound = new SoundPathSpecifier("/Audio/Machines/microwave_done_beep.ogg");

    [DataField]
    public SoundSpecifier? LoopingSound = new SoundPathSpecifier("/Audio/Machines/microwave_loop.ogg");

    [DataField]
    public SoundSpecifier? AddItemSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

    [DataField]
    public SoundSpecifier? RemoveItemSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

    [DataField]
    public string ContainerId = "fabricator_entity_container";

    [DataField, AutoNetworkedField]
    public float SecondsRemaining = 0.0f;

    [DataField, AutoNetworkedField]
    public TimeSpan EndTime;

    [DataField]
    public EntityUid? SoundStream;

    [DataField]
    public EntityUid? SoundStreamClient;

    [ViewVariables]
    public Container Storage = default!;

    [DataField, AutoNetworkedField]
    public bool FabRunning = false;

    [ViewVariables]
    public (FabricatorRecipePrototype?, int) PortionedRecipe;
}
