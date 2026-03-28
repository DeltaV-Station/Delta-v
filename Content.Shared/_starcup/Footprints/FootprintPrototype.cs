using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Shared._starcup.Footprints;

[Prototype]
public sealed partial class FootprintPrototype : IPrototype, IInheritingPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<FootprintPrototype>))]
    public string[]? Parents { get; private set; }

    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; private set; }

    /// <summary>
    /// The soft, normal maximum volume of a footprint.
    /// If a user picks up more than MaxStoredVolume then the footprint may be larger
    /// </summary>
    [DataField]
    public float MaxVolume = 1;

    /// <summary>
    /// The soft minimum volume of a footprint.
    /// The last footprint put down will be less than this.
    /// </summary>
    [DataField]
    public float MinVolume = 0.5f;

    /// <summary>
    /// The amount of volume the owner should be able to pick up with this footprint.
    /// The actual volume may be higher if picked up from another footprint
    /// </summary>
    [DataField]
    public float MaxStoredVolume = 10;

    /// <summary>
    /// How far the footprint owner needs to move before leaving another footprint.
    /// </summary>
    [DataField]
    public float Distance = 0.5f;

    /// <summary>
    /// If the footprints should follow the rotation of the owner.
    /// Otherwise they'll follow the direction of movement.
    /// </summary>
    [DataField]
    public bool LocalRotation = true;

    /// <summary>
    /// Visible footprint layers to use for this footprint
    /// </summary>
    [DataField]
    public FootprintLayerData Prints = default!;
}

[ImplicitDataDefinitionForInheritors, UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class FootprintLayerData
{
    [DataField(required: true)]
    public List<FootprintLayer> Layers = new();

    public abstract int GetLayerIndex(int index, IRobustRandom rand);
}

/// <summary>
/// Provides a series of footprints in order.
/// </summary>
public sealed partial class SequentialFootprint : FootprintLayerData
{
    public override int GetLayerIndex(int index, IRobustRandom rand)
    {
        return ++index % Layers.Count;
    }
}

/// <summary>
/// Randomly selects footprints from its list
/// </summary>
public sealed partial class RandomFootprint : FootprintLayerData
{
    public override int GetLayerIndex(int index, IRobustRandom rand)
    {
        return rand.Next(Layers.Count);
    }
}

[DataDefinition]
public sealed partial class FootprintLayer
{
    /// <summary>
    /// The state in your footprint RSI that should be used to show this print
    /// </summary>
    [DataField]
    public string State;

    /// <summary>
    /// How far left or right this footprint should be from the center of the owner
    /// This value is in relation to down being normal, so positive is left and negative is right.
    /// </summary>
    [DataField]
    public float Offset = 0;
}
