// SPDX-FileCopyrightText: 2025 Coenx-flex
// SPDX-FileCopyrightText: 2025 Cojoke
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Containers;

namespace Content.Shared._Mono.CorticalBorer;


[RegisterComponent, NetworkedComponent]
public sealed partial class CorticalBorerInfestedComponent : Robust.Shared.GameObjects.Component
{
    /// <summary>
    /// Borer in the person
    /// </summary>
    [ViewVariables]
    public Entity<CorticalBorerComponent> Borer = new();

    /// <summary>
    ///     Container for borer
    /// </summary>
    public Container InfestationContainer = new();

    /// <summary>
    /// is the person under the borer's control
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan? ControlTimeEnd;

    [ViewVariables]
    public EntityUid? OrigininalMindId;

    [ViewVariables]
    public EntityUid BorerMindId;

    /// <summary>
    /// Where the mind gets hidden when the worm takes control
    /// </summary>
    public Container ControlContainer;

    /// <summary>
    /// Abilities to be removed once host gets control back
    /// </summary>
    public List<EntityUid> RemoveAbilities = new();
}

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryCorticalBorerConditionComponent : Robust.Shared.GameObjects.Component;
