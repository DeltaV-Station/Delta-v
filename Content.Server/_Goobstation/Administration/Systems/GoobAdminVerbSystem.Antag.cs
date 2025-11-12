// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Content.Server._Goobstation.Devil.GameTicking.Rules;
using Content.Server.Administration.Managers;
using Content.Server.Antag;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Mind.Components;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server._Goobstation.Administration.Systems;

public sealed partial class GoobAdminVerbSystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IAdminManager _admin = default!;

    private void AddAntagVerbs(GetVerbsEvent<Verb> args)
    {
        if (!AntagVerbAllowed(args, out var targetPlayer))
            return;
        // Devil
        Verb devilAntag = new()
        {
            Text = Loc.GetString("admin-verb-text-make-devil"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("_Goobstation/Actions/devil.rsi"), "summon-contract"),
            Act = () =>
            {
                _antag.ForceMakeAntag<DevilRuleComponent>(targetPlayer, "Devil");
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-devil"),
        };
        args.Verbs.Add(devilAntag);
    }

    public bool AntagVerbAllowed(GetVerbsEvent<Verb> args, [NotNullWhen(true)] out ICommonSession? target)
    {
        target = null;

        if (!TryComp<ActorComponent>(args.User, out var actor))
            return false;

        var player = actor.PlayerSession;

        if (!_admin.HasAdminFlag(player, AdminFlags.Fun))
            return false;

        if (!HasComp<MindContainerComponent>(args.Target) || !TryComp<ActorComponent>(args.Target, out var targetActor))
            return false;

        target = targetActor.PlayerSession;
        return true;
    }
}
