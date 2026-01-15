using Content.Shared._DV.Tips;
using Content.Shared._DV.Trigger.Components.Effects;
using Content.Shared.Trigger;
using Robust.Shared.Player;

namespace Content.Shared._DV.Trigger.Systems;

public sealed class ShowTipOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly SharedTipSystem _tips = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShowTipOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    private void OnTrigger(Entity<ShowTipOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (args.Key != null && !ent.Comp.KeysIn.Contains(args.Key))
            return;

        var target = ent.Comp.TargetUser ? args.User : ent.Owner;

        if (target == null)
            return;

        if (!_player.TryGetSessionByEntity(target.Value, out var session))
            return;

        _tips.ShowTip(session, ent.Comp.Tip);
        args.Handled = true;
    }
}
