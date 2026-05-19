// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Goobstation.Religion;
using Content.Server._Goobstation.Bible;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Timing;
using Content.Server.Bible.Components;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes; // Shitmed Change

namespace Content.Server._Goobstation.Religion;

public sealed class WeakToHolySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly GoobBibleSystem _goobBible = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeakToHolyComponent, InteractUsingEvent>(AfterBibleUse);
        SubscribeLocalEvent<WeakToHolyComponent, MapInitEvent>(OnInit); // DeltaV - Add damage set
        SubscribeLocalEvent<WeakToHolyComponent, ComponentRemove>(OnRemove); // DeltaV - Clean up on removal
    }


    // Begin DeltaV Additions - Holy Weakness
    private void OnInit(Entity<WeakToHolyComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<DamageableComponent>(ent, out var damageable))
            return;

        var dmg = damageable.Damage;
        if (dmg.DamageDict.ContainsKey("Holy"))
        {
            ent.Comp.HadHolyWeakness = true;
            return;
        }
        dmg.DamageDict["Holy"] = 0;
    }

    private void OnRemove(Entity<WeakToHolyComponent> ent, ref ComponentRemove args)
    {
        if (!TryComp<DamageableComponent>(ent, out var damageable))
            return;

        if (!ent.Comp.HadHolyWeakness)
        {
            var dmg = damageable.Damage;
            dmg.DamageDict.Remove("Holy");
        }
    }
    // End DeltaV additions

    private void AfterBibleUse(Entity<WeakToHolyComponent> ent, ref InteractUsingEvent args)
    {
        if (!TryComp<BibleComponent>(args.Used, out var bibleComp))
            return;

        if (!TryComp(args.Used, out UseDelayComponent? useDelay)
            || _useDelay.IsDelayed((args.Used, useDelay))
            || !HasComp<BibleUserComponent>(args.User))
            return;

        _goobBible.TryDoSmite(args.Used, args.User, args.Target, useDelay);
    }

    #region Holy Healing
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Holy damage healing.
        var query = EntityQueryEnumerator<WeakToHolyComponent, BodyComponent>();
        while (query.MoveNext(out var uid, out var weakToHoly, out var body))
        {
            if (weakToHoly.NextPassiveHealTick > _timing.CurTime)
                continue;
            weakToHoly.NextPassiveHealTick = _timing.CurTime + weakToHoly.HealTickDelay;

            if (!TryComp<DamageableComponent>(uid, out var damageable))
                continue;

            if (TerminatingOrDeleted(uid)
                || _body.GetRootPartOrNull(uid, body: body) is not { }
                || !damageable.Damage.DamageDict.TryGetValue("Holy", out _))
                continue;

            // Rune healing.
            if (weakToHoly.IsColliding)
                _damageableSystem.TryChangeDamage(uid, weakToHoly.HealAmount, ignoreResistances: true, targetPart: TargetBodyPart.All);

            // Passive healing.
            _damageableSystem.TryChangeDamage(uid, weakToHoly.PassiveAmount, ignoreResistances: true, targetPart: TargetBodyPart.All);
        }
    }

    #endregion
}
