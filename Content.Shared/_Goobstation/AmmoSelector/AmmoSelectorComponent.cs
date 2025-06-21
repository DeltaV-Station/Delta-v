// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Weapons.AmmoSelector;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AmmoSelectorComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<SelectableAmmoPrototype>> Prototypes = new();

    [DataField, AutoNetworkedField]
    public SelectableAmmoPrototype? CurrentlySelected;

    [DataField]
    public SoundSpecifier? SoundSelect = new SoundPathSpecifier("/Audio/Weapons/Guns/Misc/selector.ogg");
}