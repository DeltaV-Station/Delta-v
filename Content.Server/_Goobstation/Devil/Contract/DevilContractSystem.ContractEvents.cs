// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Goobstation.Devil;
using Content.Shared.Body; // Delta V - Nubody
using Content.Shared.Body.Components;
using Robust.Shared.Containers; // Delta V - Nubody
using Robust.Shared.Random;

namespace Content.Server._Goobstation.Devil.Contract;

public sealed partial class DevilContractSystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;

    private void InitializeSpecialActions()
    {
        SubscribeLocalEvent<DevilContractSoulOwnershipEvent>(OnSoulOwnership);
        SubscribeLocalEvent<DevilContractLoseHandEvent>(OnLoseHand);
        SubscribeLocalEvent<DevilContractLoseOrganEvent>(OnLoseOrgan);
        SubscribeLocalEvent<DevilContractChanceEvent>(OnChance);
    }
    private void OnSoulOwnership(DevilContractSoulOwnershipEvent args)
    {
        if (args.Contract?.ContractOwner is not { } contractOwner)
            return;

        TryTransferSouls(contractOwner, args.Target, 1);
    }

    private void OnLoseHand(DevilContractLoseHandEvent args)
    {
        if (_hands.GetHandCount(args.Target) <= 0)
            return;

        var pick = _random.Pick(_hands.EnumerateHands(args.Target).ToList());

        _hands.RemoveHand(args.Target, pick);

        Log.Debug($"Removed part {pick} from {ToPrettyString(args.Target)}"); // DeltaV - Use EntitySystem Logger intead of _sawmill
    }

    private void OnLoseOrgan(DevilContractLoseOrganEvent args)
    {
        if (!TryComp<BodyComponent>(args.Target, out var body))
            return;

        if (body.Organs == null)
            return;

        // don't remove the brain, as funny as that is.
        var eligibleOrgans = body.Organs.ContainedEntities
            .Where(o => !HasComp<BrainComponent>(o))
            .ToList();

        if (eligibleOrgans.Count <= 0)
            return;

        var pick = _random.Pick(eligibleOrgans);

        _containerSystem.Remove(pick, body.Organs );
        Log.Debug($"Removed {pick.Id} from {ToPrettyString(args.Target)}"); // DeltaV - Use EntitySystem Logger intead of _sawmill
    }

    // LETS GO GAMBLING!!!!!
    private void OnChance(DevilContractChanceEvent args)
    {
        AddRandomClause(args.Target);
    }
}
