using Content.Shared.Chat;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Chat;

public sealed class DVDeathEmoteCooldownSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DVDeathEmoteCooldownComponent, BeforeEmoteEvent>(OnBeforeEmote);
        SubscribeLocalEvent<DVDeathEmoteCooldownComponent, EmoteEvent>(OnEmote);
    }

    private void OnEmote(Entity<DVDeathEmoteCooldownComponent> ent, ref EmoteEvent args)
    {
        if (args.Emote.Category != EmoteCategory.Dead)
            return;

        ent.Comp.CanEmoteAt = _timing.CurTime + ent.Comp.EmoteCooldown;
        Dirty(ent);
    }

    private void OnBeforeEmote(Entity<DVDeathEmoteCooldownComponent> ent, ref BeforeEmoteEvent args)
    {
        if (args.Emote.Category != EmoteCategory.Dead)
            return;

        if (ent.Comp.CanEmoteAt > _timing.CurTime)
            args.Cancel();
    }
}
