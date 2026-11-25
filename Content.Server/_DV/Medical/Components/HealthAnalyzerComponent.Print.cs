using Content.Shared.Paper;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Medical.Components;

/// <summary>
/// Extends upstream's HealthAnalyzerComponent.
/// </summary>
/// </remarks>
public sealed partial class HealthAnalyzerComponent : Component
{
    /// <summary>
    /// Time when the component can print again
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan PrintAllowedAfter = TimeSpan.Zero;

    /// <summary>
    /// Cooldown between individual prints to prevent entity and noise spam.
    /// </summary>
    [DataField]
    public TimeSpan PrintCooldown = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Prototype of an entity to use as a template for printing. The paper may contain placeholders (wrapped in braces)
    /// which will be filled in during printing.
    /// </summary>
    [DataField]
    public EntProtoId<PaperComponent> PrintTemplate = "PaperWrittenMedicalMedtekPatientRecord";

    [DataField]
    public SoundSpecifier PrintReportSound = new SoundPathSpecifier("/Audio/Machines/printer.ogg");
}
