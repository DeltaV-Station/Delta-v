using Content.Server.Abilities.Felinid;
using Content.Server.Chat.Systems;
using Content.Server.Nutrition.Components;
using Content.Server.Popups;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.Abilities.Chitinid;

public sealed partial class ChitinidSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChitinidComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ChitinidComponent,ChitziteActionEvent>(OnChitzite);
        SubscribeLocalEvent<ChitinidComponent,MapInitEvent>(OnMapInit);
    }

    private Queue<EntityUid> RemQueue = new();


    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        foreach (var bug in RemQueue)
        {
            RemComp<CoughingUpChitziteComponent>(bug);
        }
        RemQueue.Clear();

        var query = EntityQueryEnumerator<ChitinidComponent, DamageableComponent>();
        while (query.MoveNext(out var uid, out var chitinid, out var damageable))
        {
            if (_gameTiming.CurTime < chitinid.NextUpdate)
                continue;

            chitinid.NextUpdate += chitinid.UpdateInterval;

            if (chitinid.AmountAbsorbed >= chitinid.MaximumAbsorbed && !_mobState.IsDead(uid))
                continue;

            var damage = new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>("Radiation"), -0.5);
            var delta = _damageable.TryChangeDamage(uid, damage);
            if (delta != null)
            {
                chitinid.AmountAbsorbed += -(delta.GetTotal().Float());
                if (chitinid.ChitziteAction != null && chitinid.AmountAbsorbed >= chitinid.MaximumAbsorbed)
                {
                    _actionsSystem.SetCharges(chitinid.ChitziteAction, 1); // You get the charge back and that's it. Tough.
                    _actionsSystem.SetEnabled(chitinid.ChitziteAction, true);
                }
            }
        }

        foreach (var (chitziteComponent, chitinidComponent) in EntityQuery<CoughingUpChitziteComponent, ChitinidComponent>())
        {
            chitziteComponent.Accumulator += frameTime;
            if (chitziteComponent.Accumulator < chitziteComponent.CoughUpTime.TotalSeconds)
                continue;

            chitziteComponent.Accumulator = 0;
            SpawnChitzite(chitziteComponent.Owner, chitinidComponent);
            RemQueue.Enqueue(chitziteComponent.Owner);
        }
    }

    private void OnMapInit(Entity<ChitinidComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate = _gameTiming.CurTime + ent.Comp.UpdateInterval;
    }

    private void OnInit(EntityUid uid, ChitinidComponent component, ComponentInit args)
    {
        if (component.ChitziteAction != null)
            return;

        _actionsSystem.AddAction(uid, ref component.ChitziteAction, component.ChitziteActionId);
    }

    private void OnChitzite(EntityUid uid, ChitinidComponent component, ChitziteActionEvent args)
    {
        if (_inventorySystem.TryGetSlotEntity(uid, "mask", out var maskUid) &&
            EntityManager.TryGetComponent<IngestionBlockerComponent>(maskUid, out var blocker) &&
            blocker.Enabled)
        {
            _popupSystem.PopupEntity(Loc.GetString("chitzite-mask", ("mask", maskUid)), uid, uid);
            return;
        }

        _popupSystem.PopupEntity(Loc.GetString("chitzite-cough", ("name", Identity.Entity(uid, EntityManager))), uid);
        _audio.PlayPvs("/Audio/Animals/cat_hiss.ogg", uid, AudioHelpers.WithVariation(0.15f));

        EnsureComp<CoughingUpChitziteComponent>(uid);
        args.Handled = true;
    }

    private void SpawnChitzite(EntityUid uid, ChitinidComponent component)
    {
        var chitzite = EntityManager.SpawnEntity(component.ChitzitePrototype, Transform(uid).Coordinates);
        component.AmountAbsorbed = 0f;
    }

}
