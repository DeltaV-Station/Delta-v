using Content.Server.Popups;
using Content.Shared._Impstation.Supermatter.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Database;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
namespace Content.Server._Impstation.Supermatter.Systems;

public sealed partial class SupermatterSystem
{
    [Dependency] private readonly PopupSystem _popup = default!;

    /// <summary>
    /// This is used to get the energy value of items tagged as supermatter food.
    /// </summary>
    private EntityQuery<SupermatterFoodComponent> _foodQuery;

    /// <summary>
    /// This is used to avoid consuming entities with godmode.
    /// </summary>
    private EntityQuery<GodmodeComponent> _godmodeQuery;

    /// <summary>
    /// This is used to determine if an entity is immune to being consumed by the supermatter.
    /// </summary>
    private EntityQuery<SupermatterImmuneComponent> _immuneQuery;

    /// <summary>
    /// This is used for consuming mobs bumping into the supermatter
    /// </summary>
    private EntityQuery<MobStateComponent> _mobStateQuery;

    /// <summary>
    /// This is used to get the mass of items touching the supermatter.
    /// </summary>
    private EntityQuery<PhysicsComponent> _physicsQuery;

    /// <summary>
    /// This is used to convert projectile damage into supermatter power.
    /// </summary>
    private EntityQuery<ProjectileComponent> _projectileQuery;

    /// <summary>
    /// This is used to consume both the entity and the item if an otherwise undroppable item is used on the supermatter.
    /// </summary>
    private EntityQuery<UnremoveableComponent> _unremoveableQuery;

    private void InitializeCollision()
    {
        Log.Debug("Calling InitializeCollision");
        
        _foodQuery = GetEntityQuery<SupermatterFoodComponent>();
        _godmodeQuery = GetEntityQuery<GodmodeComponent>();
        _immuneQuery = GetEntityQuery<SupermatterImmuneComponent>();
        _mobStateQuery = GetEntityQuery<MobStateComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _projectileQuery = GetEntityQuery<ProjectileComponent>();
        _unremoveableQuery = GetEntityQuery<UnremoveableComponent>();
        
        SubscribeLocalEvent<SupermatterComponent, StartCollideEvent>(OnCollideEvent);
        SubscribeLocalEvent<SupermatterComponent, EmbeddedEvent>(OnEmbedded);
        SubscribeLocalEvent<SupermatterComponent, InteractHandEvent>(OnHandInteract);
        SubscribeLocalEvent<SupermatterComponent, InteractUsingEvent>(OnItemInteract);
    }

    private void OnCollideEvent(Entity<SupermatterComponent> ent, ref StartCollideEvent args)
    {
        TryCollision(ent, args.OtherEntity, args.OtherBody);
    }

    private void OnEmbedded(Entity<SupermatterComponent> ent, ref EmbeddedEvent args)
    {
        TryCollision(ent, args.Embedded, checkStatic: false);
    }

    private void OnHandInteract(Entity<SupermatterComponent> ent, ref InteractHandEvent args)
    {
        var target = args.User;

        if (_immuneQuery.HasComp(target) || _godmodeQuery.HasComp(target))
            return;

        if (!ent.Comp.HasBeenPowered)
            LogFirstPower(ent, ent.Comp, target);

        var power = 200f;

        if (_physicsQuery.TryComp(target, out var physics))
            power += physics.Mass;

        ent.Comp.MatterPower += power;

        _popup.PopupEntity(Loc.GetString("supermatter-collide-mob", ("sm", ent.Owner), ("target", target)), ent, PopupType.LargeCaution);
        Audio.PlayPvs(ent.Comp.DustSound, ent);

        // Prevent spam or excess power production
        AddComp<SupermatterImmuneComponent>(target);

        _chatManager.SendAdminAlert($"{EntityManager.ToPrettyString(ent.Owner):uid} has consumed {EntityManager.ToPrettyString(target):target}");
        _adminLog.Add(LogType.EntityDelete, LogImpact.High, $"{EntityManager.ToPrettyString(target):target} touched {EntityManager.ToPrettyString(ent.Owner):uid} and was destroyed at {Transform(ent.Owner).Coordinates:coordinates}");
        EntityManager.SpawnEntity(ent.Comp.CollisionResultPrototype, Transform(target).Coordinates);
        EntityManager.QueueDeleteEntity(target);

        args.Handled = true;
    }

    private void OnItemInteract(Entity<SupermatterComponent> ent, ref InteractUsingEvent args)
    {
        var target = args.User;
        var item = args.Used;
        var othersFilter = Filter.Pvs(ent).RemovePlayerByAttachedEntity(target);

        if (args.Handled ||
            GhostQuery.HasComp(target) ||
            _immuneQuery.HasComp(item) ||
            _godmodeQuery.HasComp(item))
            return;

        // TODO: supermatter scalpel
        if (_unremoveableQuery.HasComp(item))
        {
            if (!ent.Comp.HasBeenPowered)
                LogFirstPower(ent.Owner, ent.Comp, target);

            var power = 200f;

            if (_physicsQuery.TryComp(target, out var targetPhysics))
                power += targetPhysics.Mass;

            if (_physicsQuery.TryComp(item, out var itemPhysics))
                power += itemPhysics.Mass;

            ent.Comp.MatterPower += power;

            _popup.PopupEntity(Loc.GetString("supermatter-collide-insert-unremoveable", 
                ("target", target), ("sm", ent.Owner), ("item", item)), ent, othersFilter, true, PopupType.LargeCaution);
            _popup.PopupEntity(Loc.GetString("supermatter-collide-insert-unremoveable-user", ("sm", ent.Owner), ("item", item)), ent, target, PopupType.LargeCaution);
            Audio.PlayPvs(ent.Comp.DustSound, ent);

            // Prevent spam or excess power production
            AddComp<SupermatterImmuneComponent>(target);
            AddComp<SupermatterImmuneComponent>(item);

            _adminLog.Add(LogType.EntityDelete, LogImpact.High, $"{EntityManager.ToPrettyString(target):target} touched {EntityManager.ToPrettyString(ent.Owner):uid} with {EntityManager.ToPrettyString(item):item} and both were destroyed at {Transform(ent.Owner).Coordinates:coordinates}");
            EntityManager.SpawnEntity(ent.Comp.CollisionResultPrototype, Transform(target).Coordinates);
            EntityManager.QueueDeleteEntity(target);
            EntityManager.QueueDeleteEntity(item);
        }
        else
        {
            if (!ent.Comp.HasBeenPowered)
                LogFirstPower(ent.Owner, ent.Comp, item);

            if (_physicsQuery.TryComp(item, out var physics))
                ent.Comp.MatterPower += physics.Mass;

            _popup.PopupEntity(Loc.GetString("supermatter-collide-insert", ("target", target), ("sm", ent.Owner), ("item", item)), ent, othersFilter, true, PopupType.LargeCaution);
            _popup.PopupEntity(Loc.GetString("supermatter-collide-insert-user", ("sm", ent.Owner), ("item", item)), ent, target, PopupType.LargeCaution);
            Audio.PlayPvs(ent.Comp.DustSound, ent);

            // Prevent spam or excess power production
            AddComp<SupermatterImmuneComponent>(item);

            _adminLog.Add(LogType.EntityDelete, LogImpact.High, $"{EntityManager.ToPrettyString(target):target} touched {EntityManager.ToPrettyString(ent.Owner):uid} with {EntityManager.ToPrettyString(item):item} and destroyed it at {Transform(ent.Owner).Coordinates:coordinates}");
            EntityManager.QueueDeleteEntity(item);
        }

        args.Handled = true;
    }

    private void TryCollision(Entity<SupermatterComponent> ent, EntityUid target, PhysicsComponent? targetPhysics = null, bool checkStatic = true)
    {
        if (!Resolve(target, ref targetPhysics))
            return;

        if (targetPhysics.BodyType == BodyType.Static && checkStatic ||
            _immuneQuery.HasComp(target) ||
            _godmodeQuery.HasComp(target) ||
            Container.IsEntityInContainer(ent))
            return;

        if (!ent.Comp.HasBeenPowered)
            LogFirstPower(ent.Owner, ent.Comp, target);

        if (!_projectileQuery.HasComp(target))
        {
            var popup = "supermatter-collide";

            if (_mobStateQuery.HasComp(target))
            {
                popup = "supermatter-collide-mob";
                EntityManager.SpawnEntity(ent.Comp.CollisionResultPrototype, Transform(target).Coordinates);
                _chatManager.SendAdminAlert($"{EntityManager.ToPrettyString(ent.Owner):uid} has consumed {EntityManager.ToPrettyString(target):target}");
            }

            var targetProto = MetaData(target).EntityPrototype;
            if (targetProto != null && targetProto.ID != ent.Comp.CollisionResultPrototype)
            {
                _popup.PopupEntity(Loc.GetString(popup, ("sm", ent.Owner), ("target", target)), ent, PopupType.LargeCaution);
                Audio.PlayPvs(ent.Comp.DustSound, ent);
            }

            ent.Comp.MatterPower += targetPhysics.Mass;
            _adminLog.Add(LogType.EntityDelete, LogImpact.High, $"{EntityManager.ToPrettyString(target):target} collided with {EntityManager.ToPrettyString(ent.Owner):uid} at {Transform(ent.Owner).Coordinates:coordinates}");
        }

        // Prevent spam or excess power production
        AddComp<SupermatterImmuneComponent>(target);

        EntityManager.QueueDeleteEntity(target);

        if (_foodQuery.TryComp(target, out var food))
            ent.Comp.Power += food.Energy;
        else if (_projectileQuery.TryComp(target, out var projectile))
            ent.Comp.Power += (float) projectile.Damage.GetTotal();
        else
            ent.Comp.Power++;

        ent.Comp.MatterPower += _mobStateQuery.HasComp(target) ? 200 : 0;
        
        DirtyField(ent, ent.Comp, nameof(SupermatterComponent.Power));
    }
}
