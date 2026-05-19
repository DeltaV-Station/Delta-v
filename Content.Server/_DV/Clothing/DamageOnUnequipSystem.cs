using Content.Server.Chat.Systems;
using Content.Shared._DV.Clothing;
using Content.Shared._DV.Clothing.Components;
using Content.Shared.Clothing;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Jittering;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Server._DV.Clothing;

public sealed class DamageOnUnequipSystem : SharedDamageOnUnequipSystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageOnUnequipComponent, ClothingGotUnequippedEvent>(OnUnequip);
    }

    private void OnUnequip(Entity<DamageOnUnequipComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        // terminating check to avoid evil exception. bit weird but it's how a similar issue was fixed upstream
        if (ent.Comp.UnequipDamage == null || !TryComp<DamageableComponent>(args.Wearer, out var damageable) || MetaData(args.Wearer).EntityLifeStage >= EntityLifeStage.Terminating)
            return;

        _popup.PopupEntity(Loc.GetString("damage-on-unequip-finish", ("item", ent), ("wearer", args.Wearer)), ent, PopupType.LargeCaution);

        if (ent.Comp.UnequipSound != null)
            _audio.PlayPvs(ent.Comp.UnequipSound, ent);

        if (ent.Comp.ScreamOnUnequip)
        {
            _chat.TryEmoteWithChat(args.Wearer, ent.Comp.ScreamEmote);
            _jittering.DoJitter(args.Wearer, TimeSpan.FromSeconds(15), false);
        }

        _damageable.TryChangeDamage(args.Wearer, ent.Comp.UnequipDamage, true, true, canSever: false); // Shitmed
    }
}
