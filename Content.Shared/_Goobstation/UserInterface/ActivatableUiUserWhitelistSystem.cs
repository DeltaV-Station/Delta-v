// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.UserInterface;
using Content.Shared.Whitelist;

namespace Content.Shared._Goobstation.Wizard.UserInterface;

public sealed class ActivatableUiUserWhitelistSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActivatableUiUserWhitelistComponent, ActivatableUIOpenAttemptEvent>(OnAttempt);
    }

    private void OnAttempt(Entity<ActivatableUiUserWhitelistComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (!CheckWhitelist(ent.Owner, args.User, ent.Comp))
            args.Cancel();
    }

    public bool CheckWhitelist(EntityUid uid, EntityUid user, ActivatableUiUserWhitelistComponent? component = null)
    {
        return !Resolve(uid, ref component, false) || _whitelist.IsValid(component.Whitelist, user);
    }
}