using System.Reflection.Metadata;
using Content.Shared._DV.Trigger.Components.Effects;
using Content.Shared.Trigger;
using Robust.Shared.Player;
using Content.Server._DV.Abilities;

namespace Content.Shared._DV.Trigger.Systems;

public sealed class CreateHitmanCardOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly SpawnHitmanCardSystem _SpawnHitmanCardSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CreateHitmanCardOnTriggerComponent, TriggerEvent>(OnTrigger);
    }
    //just spawns a card
    private void OnTrigger(Entity<CreateHitmanCardOnTriggerComponent> ent, EntityUid target, ref TriggerEvent args)
    {
        if (target == null)
        {
            return;
        }

        var owner = (EntityUid)target;
        var coords = (Coordinates)Transform(owner).Coordinates;
        _SpawnHitmanCardSystem.CreateHitmanCard(owner,coords);
        args.Handled = true;
    }
}
