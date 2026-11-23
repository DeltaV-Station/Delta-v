using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared._DV.Weapons.Ranged.Attachments;

public sealed class GunDamageModifierAttachmentSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunDamageModifierAttachmentComponent, AmmoShotEvent>(OnShot);
        SubscribeLocalEvent<GunDamageModifierAttachmentComponent, ExaminedEvent>(OnAttachmentExamined);
    }

    private void OnShot(Entity<GunDamageModifierAttachmentComponent> ent, ref AmmoShotEvent args)
    {
        foreach (var uid in args.FiredProjectiles)
        {
            if (TryComp<ProjectileComponent>(uid, out var projectile))
                projectile.Damage *= ent.Comp.Multiplier;
        }
    }

    private void OnAttachmentExamined(Entity<GunDamageModifierAttachmentComponent> ent, ref ExaminedEvent args)
    {
        Color GetColor(float m) => m > 1 ? Color.Crimson : m < 1 ? Color.Lime : Color.Gold; // thanks mono

        args.PushMarkup(Loc.GetString("gun-damage-modifier-attachment-component-examine", ("color", GetColor(ent.Comp.Multiplier)), ("multiplier", ent.Comp.Multiplier)));
    }
}
