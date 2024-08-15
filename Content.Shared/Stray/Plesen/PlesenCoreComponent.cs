using Robust.Shared.GameStates;
using Content.Shared.Stray.Plesen.PlesenWall;
using Content.Shared.Stray.Plesen.PlesenFloor;
using Content.Shared.Stray.Plesen.PlesenCocone;
using Robust.Shared.Serialization;


namespace Content.Shared.Stray.Plesen.PlesenCore;

//[Serializable, NetSerializable]
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(true)]
[Access(typeof(SharedPlesenCoreSystem))]
public sealed partial class PlesenCoreComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float nextUpdateTime = 2;

    public TimeSpan updateTime = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadOnly)]
    [AutoNetworkedField]
    public bool fullyGroth = false;

    public TimeSpan growAfter = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float plesenStress = 0;

    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float energy = 5;

    [ViewVariables(VVAccess.ReadOnly)]
    public float avarageEnergy = 10;

    [ViewVariables(VVAccess.ReadOnly)]
    public float avarageHealth = 10;

    //[ViewVariables(VVAccess.ReadOnly)]
    public float lastAvarageHealth = 10;

    [ViewVariables(VVAccess.ReadOnly)]
    public float avarageCoresHealth = 10;

    [DataField("health"),ViewVariables(VVAccess.ReadWrite)]
    public float health = 100;

    [ViewVariables(VVAccess.ReadOnly)]
    public float realHealth = 100;

    //[ViewVariables(VVAccess.ReadOnly)]
    public List<PlesenCoreComponent> attachedCores = new List<PlesenCoreComponent>();

    //[ViewVariables(VVAccess.ReadOnly)]
    public List<PlesenWallComponent> attachedWalls = new List<PlesenWallComponent>();

    //[ViewVariables(VVAccess.ReadOnly)]
    public List<PlesenFloorComponent> attachedFloors = new List<PlesenFloorComponent>();

    //[ViewVariables(VVAccess.ReadOnly)]
    public List<PlesenCoconeComponent> attachedCocones = new List<PlesenCoconeComponent>();
    [ViewVariables(VVAccess.ReadWrite)]
    public int totalSpawnedCoresCount = 0;
    [ViewVariables(VVAccess.ReadWrite)]
    public int totalSpawnedFloorsCount = 0;
    [ViewVariables(VVAccess.ReadWrite)]
    public int totalSpawnedWallsCount = 0;
    [ViewVariables(VVAccess.ReadWrite)]
    public int totalSpawnedCoconesCount = 0;


}
