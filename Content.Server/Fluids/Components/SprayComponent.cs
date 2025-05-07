using Content.Server.Fluids.EntitySystems;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Content.Shared.Inventory; // DeltaV - ATMOS Extinguisher Nozzle
using Content.Shared.Whitelist; // DeltaV - ATMOS Extinguisher Nozzle

namespace Content.Server.Fluids.Components;

[RegisterComponent]
[Access(typeof(SpraySystem))]
public sealed partial class SprayComponent : Component
{
    public const string SolutionName = "spray";
    public const string TankSolutionName = "tank"; //  DeltaV - ATMOS Extinguisher Nozzle

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public FixedPoint2 TransferAmount = 10;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float SprayDistance = 3.5f;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float SprayVelocity = 3.5f;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public EntProtoId SprayedPrototype = "Vapor";

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int VaporAmount = 1;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float VaporSpread = 90f;

    /// <summary>
    /// How much the player is pushed back for each spray.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float PushbackAmount = 2f;

    [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
    [Access(typeof(SpraySystem), Other = AccessPermissions.ReadExecute)] // FIXME Friends
    public SoundSpecifier SpraySound { get; private set; } = default!;

    /// <remarks>
    /// Begin DeltaV Additions - ATMOS Extinguisher Nozzle 
    /// </remarks>
    [DataField]
    public SlotFlags TargetSlot;

    [DataField]
    public EntityWhitelist? ProviderWhitelist;

    [DataField]
    public bool ExternalContainer;
    // End DeltaV Additions
}
