using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Shared.Movement.Systems;

public sealed partial class MovementModStatusSystem : EntitySystem
{
    public static readonly EntProtoId PsionicSlowdown = "PsionicSlowdownStatusEffect";
}
