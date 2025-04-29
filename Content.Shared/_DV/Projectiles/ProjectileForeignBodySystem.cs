using Content.Shared._Shitmed.Targeting;
using Content.Shared.Administration.Logs;
using Content.Shared.Armor;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Projectiles;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Projectiles;

public sealed class ProjectileForeignBodySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ProjectileForeignBodyComponent, ProjectileHitEvent>(OnEmbedProjectileHit);

        SubscribeLocalEvent<BodyPartForeignBodyContainerComponent, ComponentInit>(OnBodyPartInit);
        SubscribeLocalEvent<BodyComponent, ProjectileForeignBodyAttemptEvent>(OnBodyEmbed);
        SubscribeLocalEvent<BodyPartForeignBodyContainerComponent, ForeignBodyEffectsEvent>(OnBodyPartEffect);
    }

    private void OnEmbedProjectileHit(Entity<ProjectileForeignBodyComponent> ent, ref ProjectileHitEvent args)
    {
        if (!TryComp(ent, out ProjectileComponent? projectile))
            return;

        var evt = new CoefficientQueryEvent(SlotFlags.All);
        RaiseLocalEvent(args.Target, evt);

        var chance = ent.Comp.BaseChance * evt.DamageModifiers.Coefficients.GetValueOrDefault(ent.Comp.DamageType, 1);
        if (_random.NextFloat() > chance)
            return;

        var ev = new ProjectileForeignBodyAttemptEvent(projectile.Shooter!.Value, projectile.Weapon!.Value, args.Target, ent);
        RaiseLocalEvent(args.Target, ref ev);
    }

    private void OnBodyEmbed(Entity<BodyComponent> ent, ref ProjectileForeignBodyAttemptEvent args)
    {
        if (_net.IsClient) // i yearn for predicted spawning
            return;

        var targetPart = _body.GetRandomBodyPart(args.Shooter) ?? TargetBodyPart.Torso;

        if (TryComp<TargetingComponent>(args.Shooter, out var targeting))
        {
            targetPart = targeting.Target;
            if (targetPart == TargetBodyPart.Torso)
            {
                var otherPart = SharedBodySystem.GetRandomPartSpread(_random, 10);
                targetPart = otherPart;
            }

        }

        var (targetType, targetSymmetry) = _body.ConvertTargetBodyPart(targetPart);
        foreach (var part in _body.GetBodyChildrenOfType(ent, targetType, symmetry: targetSymmetry))
        {
            if (!TryComp<BodyPartForeignBodyContainerComponent>(part.Id, out var embeddableBodyPart))
                continue;

            if (!TrySpawnInContainer(args.Embedded.Comp.ForeignBody, part.Id, embeddableBodyPart.ContainerName, out var uid))
                continue;

            var active = EnsureComp<ForeignBodyActivelyEmbeddedComponent>(uid.Value);
            active.ActiveAfter = _timing.CurTime + args.Embedded.Comp.EffectsBeginAfter;

            return;
        }
    }

    private void OnBodyPartInit(Entity<BodyPartForeignBodyContainerComponent> ent, ref ComponentInit args)
    {
        ent.Comp.Container = _container.EnsureContainer<Container>(ent, ent.Comp.ContainerName);
    }

    private void ApplyBodyPartEffects(EntityUid? body, EntityUid applyTo, Entity<ForeignBodyEffectsComponent> embedded)
    {
        if (!embedded.Comp.WorksOnTheDead && _mobState.IsDead(body!.Value))
            return;

        var effectArgs = new EntityEffectBaseArgs(applyTo, EntityManager);

        foreach (var effect in embedded.Comp.Effects)
        {
            if (!effect.ShouldApply(effectArgs, _random))
                continue;

            if (effect.ShouldLog)
            {
                _adminLog.Add(
                    LogType.ReagentEffect,
                    effect.LogImpact,
                    $"Embedded effect {effect.GetType().Name:effect}"
                    + $" applied on entity {applyTo:entity}"
                    + $" at {Transform(applyTo).Coordinates:coordinates}"
                );
            }

            effect.Effect(effectArgs);
        }
    }

    private void OnBodyPartEffect(Entity<BodyPartForeignBodyContainerComponent> ent, ref ForeignBodyEffectsEvent args)
    {
        if (!TryComp<BodyPartComponent>(ent, out var part))
            return;

        ApplyBodyPartEffects(part.Body, ent, args.Embedded);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<ForeignBodyEffectsComponent, ForeignBodyActivelyEmbeddedComponent>();
        while (query.MoveNext(out var uid, out var comp, out var active))
        {
            if (_timing.CurTime < active.ActiveAfter)
            {
                comp.NextUpdate = _timing.CurTime + comp.UpdateInterval;
                continue;
            }

            if (_timing.CurTime < comp.NextUpdate)
                continue;

            comp.NextUpdate += comp.UpdateInterval;

            if (!_container.TryGetContainingContainer((uid, null, null), out var container))
                continue;

            var evt = new ForeignBodyEffectsEvent((uid, comp));
            RaiseLocalEvent(container.Owner, ref evt);
        }
    }
}
