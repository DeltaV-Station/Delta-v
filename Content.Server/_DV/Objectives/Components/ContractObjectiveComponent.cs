using Content.Server._DV.Objectives.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Store;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Objectives.Components;

/// <summary>
/// Makes this objective part of a syndicate contract, granting TC and reputation upon completion.
/// </summary>
[RegisterComponent, Access(typeof(ContractObjectiveSystem))]
public sealed partial class ContractObjectiveComponent : Component
{
    /// <summary>
    /// How much reputation to add when completed.
    /// </summary>
    [DataField]
    public int Reputation;

    /// <summary>
    /// How much currency to give when completed.
    /// </summary>
    [DataField]
    public FixedPoint2 Payment;

    /// <summary>
    /// Pay when the contract is taken but disable rejecting it.
    /// </summary>
    [DataField]
    public bool Prepaid;

    /// <summary>
    /// Whether this contract can be rejected.
    /// Funded contracts cannot be rejected to prevent infinite TC exploiting.
    /// </summary>
    [ViewVariables]
    public bool Rejectable => !Prepaid;

    /// <summary>
    /// What currency to add.
    /// </summary>
    [DataField]
    public ProtoId<CurrencyPrototype> Currency = "Telecrystal";

    /// <summary>
    /// The mind with <c>Contracts</c> this contract belongs to.
    /// </summary>
    [DataField]
    public EntityUid? Contracts;
}
