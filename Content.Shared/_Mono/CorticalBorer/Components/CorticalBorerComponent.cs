// SPDX-FileCopyrightText: 2025 Coenx-flex
// SPDX-FileCopyrightText: 2025 Cojoke
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Mono.CorticalBorer;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CorticalBorerComponent : Component
{
    /// <summary>
    /// Host of this Borer
    /// </summary>
    [ViewVariables]
    public EntityUid? Host = null;

    /// <summary>
    /// Current number of chemical points this Borer has, used to level up and buy chems
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    [DataField]
    public int ChemicalPoints = 50;

    /// <summary>
    /// Chemicals added every second WHILE IN A HOST
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public int ChemicalGenerationRate = 1;

    /// <summary>
    /// Max Chemicals that can be held
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public int ChemicalPointCap = 250;

    /// <summary>
    /// Reagent injection amount
    /// </summary>
    public int InjectAmount = 10;

    /// <summary>
    /// At what interval does the chem ui update
    /// </summary>
    public int UiUpdateInterval = 5; // every 6 to prevent constant update on cap

    /// <summary>
    /// The max duration you can take control of your host
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public TimeSpan ControlDuration = TimeSpan.FromSeconds(40);

    /// <summary>
    ///     Cooldown between chem regen events.
    /// </summary>
    public TimeSpan UpdateTimer = TimeSpan.Zero;
    public float UpdateCooldown = 1f;

    /// <summary>
    /// Can this borer make more
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public bool CanReproduce = true;

    /// <summary>
    /// What does it vomit out of its mouth when it lays an egg
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public string EggProto = "CorticalBorerEgg";

    /// <summary>
    /// cost to lay an egg... will not update ability desc if changed
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public int EggCost = 200;

    [DataField]
    public bool ControlingHost;

    [DataField]
    public ComponentRegistry? AddOnInfest;

    [DataField]
    public ComponentRegistry? RemoveOnInfest;

    [DataField]
    public ProtoId<AlertPrototype> ChemicalAlert = "Chemicals";

    public readonly List<EntProtoId> InitialCorticalBorerActions = new()
    {
        "ActionCorticalBorerInfest",
        "ActionCorticalBorerEject",
        "ActionCorticalBorerChemMenu",
        "ActionCheckBlood",
        "ActionControlHost",
    };
}


