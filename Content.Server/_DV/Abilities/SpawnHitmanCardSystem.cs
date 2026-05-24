using System.Reflection.Metadata;
using Content.Shared._DV.Trigger.Components.Effects;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Trigger;
using Content.Shared._DV.Trigger.Systems;
using Robust.Shared.Player;
using Robust.Server.GameObjects;

namespace Content.Server._DV.Abilities;

public sealed partial class SpawnHitmanCardSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    public override void Initialize()
    {
        base.Initialize();
    }

    private void SpawnHitmanCard(EntityUid owner, Coordinates coords)
    {
        EntityUid? card = Spawn("HitmanBusinessCard", coords);
        _transform.DropNextTo(card, coords);
        _hands.TryPickupAnyHand(owner, card);
    }
}
