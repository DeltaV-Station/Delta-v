// SPDX-FileCopyrightText: 2025 Coenx-flex
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Mono.CorticalBorer;
using Content.Shared.Alert.Components;

namespace Content.Client._Mono.CorticalBorer;

/// <inheritdoc/>
public sealed class CorticalBorerSystem : SharedCorticalBorerSystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CorticalBorerComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmount);
    }

    private void OnGetCounterAmount(Entity<CorticalBorerComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.ChemicalAlert != args.Alert)
            return;

        args.Amount = ent.Comp.ChemicalPoints;
    }
}
