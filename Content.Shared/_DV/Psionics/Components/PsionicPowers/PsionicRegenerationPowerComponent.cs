using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PsionicRegenerationPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId { get; set; } = "ActionPsionicRegeneration";

    public override string PowerName { get; set; } = "psionic-power-name-psionic-regeneration";

    public override int MinGlimmerChanged { get; set; } = 20;

    public override int MaxGlimmerChanged { get; set; } = 30;

    /// <summary>
    /// How much prometheum essence will be injected into the psionic on full completion.
    /// </summary>
    [DataField]
    public float EssenceAmount = 20;

    /// <summary>
    /// The reagent ID that will be inserted into the bloodstream.
    /// </summary>
    [DataField]
    public string ReagentId = "Prometheum";

    /// <summary>
    /// How long the DoAfter lasts.
    /// </summary>
    [DataField]
    public float UseDelay = 8f;

    /// <summary>
    /// The sound that plays when activating the ability.
    /// </summary>
    [DataField]
    public SoundSpecifier SoundUse = new SoundPathSpecifier("/Audio/Nyanotrasen/Psionics/heartbeat_fast.ogg");
}
