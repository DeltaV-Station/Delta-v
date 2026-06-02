using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;
namespace Content.Shared._DV.Kitchen.BUI;

[NetSerializable, Serializable]
public sealed class DeepFryerBoundUserInterfaceState : BoundUserInterfaceState
{
    public required NetEntity[] CookingItems { get; set; }
    
    public float OilQuality { get; set; }
    
    public FixedPoint2 MinimumVolume { get; set; }
    
    public FixedPoint2 SolutionVolume { get; set; }
    
    public FixedPoint2 SolutionMaxVolume { get; set; }
    
    public int Capacity { get; set; }
    
    public Color SolutionColor { get; set; }
    
    public bool IsPowered { get; set; }
}
