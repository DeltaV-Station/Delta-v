// SPDX-FileCopyrightText: 2025 Coenx-flex
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Shared._Mono.CorticalBorer;

public sealed partial class CorticalInfestEvent : EntityTargetActionEvent { }
public sealed partial class CorticalEjectEvent : InstantActionEvent { }
public sealed partial class CorticalChemMenuActionEvent : InstantActionEvent {}
public sealed partial class CorticalCheckBloodEvent : InstantActionEvent {}
public sealed partial class CorticalTakeControlEvent : InstantActionEvent {}
public sealed partial class CorticalEndControlEvent : InstantActionEvent {}
public sealed partial class CorticalLayEggEvent : InstantActionEvent {}
