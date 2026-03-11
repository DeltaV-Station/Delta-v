using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PsionicRegenerationPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId => "ActionPsionicRegeneration";

    public override string PowerName => "psionic-power-name-psionic-regeneration";

    public override int MinGlimmerChanged => 20;

    public override int MaxGlimmerChanged => 30;

    /// <summary>
    /// How much prometheum essence will be injected into the psionic on full completion.
    /// </summary>
    [DataField]
    public float EssenceAmount = 20;

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


