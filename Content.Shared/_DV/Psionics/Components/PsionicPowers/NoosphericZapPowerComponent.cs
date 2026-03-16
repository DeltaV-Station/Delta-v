using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NoosphericZapPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId { get; set; } = "ActionNoosphericZap";

    public override string PowerName { get; set; } = "psionic-power-name-noospheric-zap";

    public override int MinGlimmerChanged { get; set; } = 10;

    public override int MaxGlimmerChanged { get; set; } = 50;

    /// <summary>
    /// The prototype for the lightning.
    /// </summary>
    [DataField]
    public EntProtoId LightningPrototpyeId = "PsionicLightning";

    /// <summary>
    /// How much damage the lightning will do if the target isn't insulated.
    /// </summary>
    [DataField]
    public int ShockDamage = 15;

    /// <summary>
    /// How long the target will be stunned.
    /// </summary>
    [DataField]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(3);
}
