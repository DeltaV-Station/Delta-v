using System.Numerics;
using Content.Shared._EE.FootPrint.Systems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._EE.FootPrint;

/// <summary>
/// Component for entities that can leave footprints as they move.
/// Tracks color state, position, and configuration for footprint generation.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(FootPrintsSystem), typeof(PuddleFootPrintsSystem))]
[AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class FootPrintsComponent : Component
{
    /// <summary>
    /// Path to the RSI containing footprint sprites.
    /// </summary>
    [DataField]
    public ResPath RsiPath = new("/Textures/_EE/Effects/footprints.rsi");

    #region Sprite State Names

    [DataField]
    public string LeftBarePrint = "footprint-left-bare-human";

    [DataField]
    public string RightBarePrint = "footprint-right-bare-human";

    [DataField]
    public string ShoesPrint = "footprint-shoes";

    [DataField]
    public string SuitPrint = "footprint-suit";

    [DataField]
    public string[] DraggingPrint =
    [
        "dragging-1",
        "dragging-2",
        "dragging-3",
        "dragging-4",
        "dragging-5",
    ];

    #endregion

    /// <summary>
    /// Prototype ID for spawned footprint entities.
    /// </summary>
    [DataField]
    public EntProtoId<FootPrintComponent> StepProtoId = "Footstep";

    /// <summary>
    /// The amount of solution to transfer with each footprint when stepping into a puddle.
    /// </summary>
    [DataField]
    public FixedPoint2 AmountToTransfer = 0.01;

    /// <summary>
    /// Current color of footprints being left. Alpha channel determines visibility.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color PrintsColor = Color.FromHex("#00000000");

    /// <summary>
    /// The size scaling factor for footprint steps. Must be positive.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StepSize = 0.7f;

    /// <summary>
    /// The size scaling factor for drag marks. Must be positive.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DragSize = 0.5f;

    /// <summary>
    /// The total amount of color accumulated from stepping in puddles.
    /// Used to determine when color should start fading.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ColorQuantity;

    /// <summary>
    /// The factor by which the alpha channel is reduced with each footprint.
    /// Higher values = faster fading.
    /// </summary>
    [DataField]
    public float ColorReduceAlpha = 0.1f;

    /// <summary>
    /// The reagent ID to transfer to footprint entities, set when stepping in puddles.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<ReagentPrototype>? ReagentToTransfer;

    /// <summary>
    /// Offset applied to footprints perpendicular to movement direction.
    /// Creates the left/right alternating pattern.
    /// </summary>
    [DataField]
    public Vector2 OffsetPrint = new(0.1f, 0f);

    /// <summary>
    /// Tracks which foot should make the next print. True for right foot, false for left.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool RightStep = true;

    /// <summary>
    /// The position of the last footprint in local coordinates.
    /// Used to determine when enough distance has been traveled for the next print.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Vector2 StepPos = Vector2.Zero;

    /// <summary>
    /// Controls how quickly the footprint color transitions when stepping in new puddles.
    /// Value between 0 and 1, where higher values mean faster color changes.
    /// </summary>
    [DataField]
    public float ColorInterpolationFactor = 0.2f;
}
