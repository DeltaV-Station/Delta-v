using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.Light.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Silicons;

public abstract class SharedStationAiShopSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiShopComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StationAiShopComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<StationAiShopComponent, StationAiRgbLightingActionEvent>(OnRgbLighting);
        SubscribeLocalEvent<StationAiShopComponent, StationAiPlaySoundActionEvent>(OnPlaySound);
        SubscribeLocalEvent<StationAiShopComponent, StationAiHealthChangeActionEvent>(OnHealthChange);
        SubscribeLocalEvent<StationAiShopComponent, StationAiSpawnEntityActionEvent>(OnHolopointer);
    }

    private void OnMapInit(Entity<StationAiShopComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ShopAction, ent.Comp.ShopActionId);
        Dirty(ent);
    }

    private void OnShutdown(Entity<StationAiShopComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ShopAction);
    }

    private void OnRgbLighting(Entity<StationAiShopComponent> ent, ref StationAiRgbLightingActionEvent args)
    {
        if (!RemComp<RgbLightControllerComponent>(args.Target))
            AddComp<RgbLightControllerComponent>(args.Target);

        args.Handled = true;
    }

    private void OnPlaySound(Entity<StationAiShopComponent> ent, ref StationAiPlaySoundActionEvent args)
    {
        _audio.PlayPredicted(args.Sound, args.Target, ent);
        args.Handled = true;
    }

    private void OnHealthChange(Entity<StationAiShopComponent> ent, ref StationAiHealthChangeActionEvent args)
    {
        _damage.TryChangeDamage(args.Target, args.Damage, origin: ent);
        args.Handled = true;
    }

    private void OnHolopointer(Entity<StationAiShopComponent> ent, ref StationAiSpawnEntityActionEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        Spawn(args.Entity, args.Target);
        args.Handled = true;
    }
}
