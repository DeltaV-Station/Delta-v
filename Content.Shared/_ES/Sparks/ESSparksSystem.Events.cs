using Content.Shared._ES.Sparks.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Projectiles;
using Content.Shared.Trigger;
using Robust.Shared.Spawners;

namespace Content.Shared._ES.Sparks;

public sealed partial class ESSparksSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ESSparkOnHitComponent, DamageChangedEvent>(OnDamaged);
        SubscribeLocalEvent<ESSparkOnItemToggleComponent, ItemToggledEvent>(OnItemToggled);
        SubscribeLocalEvent<ESSparkOnProjectileHitComponent, ProjectileHitEvent>(OnProjectileHit);
        SubscribeLocalEvent<ESSparkOnDespawnComponent, TimedDespawnEvent>(OnDespawn);
        SubscribeLocalEvent<ESSparkOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    private void OnDamaged(Entity<ESSparkOnHitComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageDelta is null)
            return;

        if (args.DamageDelta.GetTotal() < ent.Comp.Threshold)
            return;

        DoSparks(ent, user: args.Origin);
    }

    private void OnItemToggled(Entity<ESSparkOnItemToggleComponent> ent, ref ItemToggledEvent args)
    {
        if (args.Activated != ent.Comp.ActivatedSpark)
            return;
        DoSparks(ent, user: args.User);
    }

    private void OnProjectileHit(Entity<ESSparkOnProjectileHitComponent> ent, ref ProjectileHitEvent args)
    {
        DoSparks(ent, args.Shooter);
    }

    private void OnDespawn(Entity<ESSparkOnDespawnComponent> ent, ref TimedDespawnEvent args)
    {
        DoSparks(ent);
    }

    private void OnTrigger(Entity<ESSparkOnTriggerComponent> ent, ref TriggerEvent args)
    {
        DoSparks(ent);
    }
}
