using Content.Shared._DV.Traitor; // DeltaV
using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Cargo.BUI;

[NetSerializable, Serializable]
public sealed class CargoConsoleInterfaceState : BoundUserInterfaceState
{
    public string Name;
    public int Count;
    public int Capacity;
    public NetEntity Station;
    public List<CargoOrderData> Orders;
    public List<RansomData> Ransoms; // DeltaV
    public List<ProtoId<CargoProductPrototype>> Products;

    // DeltaV - added ransoms
    public CargoConsoleInterfaceState(string name, int count, int capacity, NetEntity station, List<CargoOrderData> orders, List<RansomData> ransoms, List<ProtoId<CargoProductPrototype>> products)
    {
        Name = name;
        Count = count;
        Capacity = capacity;
        Station = station;
        Orders = orders;
        Ransoms = ransoms; // DeltaV
        Products = products;
    }
}
