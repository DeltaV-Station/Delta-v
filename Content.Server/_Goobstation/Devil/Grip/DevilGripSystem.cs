// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tim <timfalken@hotmail.com>
// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Devil;
using Content.Shared._Goobstation.Religion;
using Content.Server.Chat.Systems;
using Content.Server.Speech.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Damage.Systems;
using Content.Shared.Interaction;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Goobstation.Devil.Grip;

public sealed class DevilGripSystem : EntitySystem
{
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly RatvarianLanguageSystem _language = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DevilGripComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(Entity<DevilGripComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach
            || args.Target is not { } target
            || args.Target == args.User
            || _whitelist.IsWhitelistPass(ent.Comp.Blacklist, target)
            || !TryComp<DevilComponent>(args.User, out var devilComp))
            return;

        // DeltaV - No Recall or Intervention can work in this place, there is no escape.
        // if (_divineIntervention.ShouldDeny(target))
        // {
        //     _actions.SetCooldown(devilComp.DevilGrip, ent.Comp.CooldownAfterUse);
        //     devilComp.DevilGrip = null;
        //     InvokeGrasp(args.User, ent);
        //     QueueDel(ent);
        //     args.Handled = true;
        //     return;
        // }

        if (TryComp(target, out StatusEffectsComponent? status))
        {
            _ = _stun.TryKnockdown(target, ent.Comp.KnockdownTime, true) || _stun.TryAddStunDuration(target, ent.Comp.KnockdownTime);
            _stamina.TakeStaminaDamage(target, ent.Comp.StaminaDamage);
            _language.DoRatvarian(target, ent.Comp.SpeechTime, true, status);
        }

        _actions.SetCooldown(devilComp.DevilGrip, ent.Comp.CooldownAfterUse);
        devilComp.DevilGrip = null;
        InvokeGrasp(args.User, ent);
        QueueDel(ent);
        args.Handled = true;
    }

    public void InvokeGrasp(EntityUid user, Entity<DevilGripComponent> ent)
    {
        _audio.PlayPvs(ent.Comp.Sound, user);
        _chat.TrySendInGameICMessage(user, Loc.GetString(ent.Comp.Invocation), InGameICChatType.Speak, false);
    }
}
