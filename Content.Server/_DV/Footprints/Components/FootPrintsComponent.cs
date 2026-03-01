using System.Numerics;
using Content.Shared.FixedPoint;

namespace Content.Server._DV.Footprints.Components;

/**
 * <summary>
 * Attach to entities that should be able to make footprints after leaving a puddle.
 * </summary>
 */
[RegisterComponent]
public sealed partial class FootPrintsComponent : Component
{
    /// <summary>
    /// Current color of footprints being left. Alpha channel determines visibility.
    /// </summary>
    [DataField]
    public Color PrintsColor = Color.FromHex("#00000000");

    /// <summary>
    /// The position of the last footprint in local coordinates.
    /// Used to determine when enough distance has been traveled for the next print.
    /// </summary>
    [DataField]
    public Vector2 LastPrintPosition = Vector2.Zero;

    /// <summary>
    /// The size scaling factor for footprint steps. Must be positive.
    /// </summary>
    [DataField]
    public float StepSize = 0.7f;

    /// <summary>
    /// The size scaling factor for drag marks. Must be positive.
    /// </summary>
    [DataField]
    public float DragSize = 0.5f;

    /// <summary>
    /// The factor by which the alpha channel is reduced with each footprint.
    /// Higher values = faster fading.
    /// </summary>
    [DataField]
    public float ColorReduceAlpha = 0.1f;

    /// <summary>
    /// Tracks which foot should make the next print. True for right foot, false for left.
    /// </summary>
    [DataField]
    public bool RightStep = true;

    /// <summary>
    /// Offset applied to footprints perpendicular to movement direction.
    /// Creates the left/right alternating pattern.
    /// </summary>
    [DataField]
    public Vector2 OffsetPrint = new(0.1f, 0f);

    /// <summary>
    /// Controls how quickly the footprint color transitions when stepping in new puddles.
    /// Value between 0 and 1, where higher values mean faster color changes.
    /// </summary>
    [DataField]
    public float ColorInterpolationFactor = 0.2f;


    /// <summary>
    /// The total amount of color accumulated from stepping in puddles.
    /// Used to determine when color should start fading.
    /// </summary>
    [DataField]
    public float ColorQuantity;

    /// <summary>
    /// The amount of solution to transfer with each footprint when stepping into a puddle.
    /// </summary>
    [DataField]
    public FixedPoint2 AmountToTransfer = 0.01;

    /// <summary>
    /// The decal to spawn for footprints.
    /// </summary>
    [DataField]
    public string PrintDecal = "footprint-shoes";

    /// <summary>
    /// The decal to spawn for being dragged.
    /// </summary>
    [DataField]
    public string[] DraggingDecals = ["smear-1", "smear-2", "smear-3", "smear-4", "smear-5"];
}
