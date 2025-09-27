using Content.Server.Chat.Systems;
using Content.Shared._DV.Clothing;
using Content.Shared._DV.Clothing.Components;
using Content.Shared.Clothing;
using Content.Shared.Damage;
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
        if (ent.Comp.UnequipDamage == null || !TryComp<DamageableComponent>(args.Wearer, out var damageable))
            return;

        _popup.PopupEntity(Loc.GetString("damage-on-unequip-finish", ("item", ent), ("wearer", args.Wearer)), ent, PopupType.LargeCaution);

        if (ent.Comp.UnequipSound != null) // this still plays twice for some reason but it's whatever
            _audio.PlayPvs(ent.Comp.UnequipSound, ent);

        if (ent.Comp.ScreamOnUnequip) // can't actually scream from Shared so OH WAIT WE'RE IN SERVER NOW
        {
            _chat.TryEmoteWithChat(args.Wearer, ent.Comp.ScreamEmote);
            _jittering.DoJitter(args.Wearer, TimeSpan.FromSeconds(15), false);
        }

        _damageable.TryChangeDamage(args.Wearer, ent.Comp.UnequipDamage, true, true, damageable);
    }
}
