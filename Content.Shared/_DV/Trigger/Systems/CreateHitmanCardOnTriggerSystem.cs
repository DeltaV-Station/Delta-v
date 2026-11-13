using Content.Shared._DV.Trigger.Components.Effects;
using Content.Shared.Trigger;
using Content.Shared.Hands.EntitySystems;

namespace Content.Shared._DV.Trigger.Systems;

public sealed class CreateHitmanCardOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CreateHitmanCardOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    //just spawns a card on the given target. uses predicated or spawn next to based on if. as trigger effect doesnt seem to have predicted on it ill just check if this is sever first and if not assume its predicted.
    private void OnTrigger(Entity<CreateHitmanCardOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (args.Handled || args.User is not { } user)
            return;

        var mapCoords = _transform.GetMapCoordinates(ent);
        var card = EntityManager.PredictedSpawn("HitmanBusinessCard", mapCoords);
        _hands.TryPickupAnyHand(user, card);
        args.Handled = true;
    }
}
