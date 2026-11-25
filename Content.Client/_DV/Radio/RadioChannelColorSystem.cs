using Content.Shared._DV.CCVars;
using Content.Shared.Radio;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.Radio;

public sealed partial class RadioChannelColorSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    // Feels like there should be a better way to do this but I'd rather not add a client-side cvar ID or reference
    // to a prototype in Content.Shared, since I don't want people thinking they can ever use it on the server-side.
    private static readonly ProtoId<RadioChannelPrototype> CommonChannelProto = "Common";
    private static readonly ProtoId<RadioChannelPrototype> CommandChannelProto = "Command";
    private static readonly ProtoId<RadioChannelPrototype> SecurityChannelProto = "Security";
    private static readonly ProtoId<RadioChannelPrototype> MedicalChannelProto = "Medical";
    private static readonly ProtoId<RadioChannelPrototype> EngineeringChannelProto = "Engineering";
    private static readonly ProtoId<RadioChannelPrototype> ScienceChannelProto = "Science";
    private static readonly ProtoId<RadioChannelPrototype> CargoChannelProto = "Supply";
    private static readonly ProtoId<RadioChannelPrototype> ServiceChannelProto = "Service";
    private static readonly ProtoId<RadioChannelPrototype> BinaryChannelProto = "Binary";
    private static readonly ProtoId<RadioChannelPrototype> JusticeChannelProto = "Justice";
    private static readonly ProtoId<RadioChannelPrototype> PrisonChannelProto = "Prison";

    public override void Initialize()
    {
        base.Initialize();

        _config.OnValueChanged(DCCVars.RadioColorCommon, (value) => { SetChannelColor(CommonChannelProto, value); }, true);
        _config.OnValueChanged(DCCVars.RadioColorCommand, (value) => { SetChannelColor(CommandChannelProto, value); }, true);
        _config.OnValueChanged(DCCVars.RadioColorSecurity, (value) => { SetChannelColor(SecurityChannelProto, value); }, true);
        _config.OnValueChanged(DCCVars.RadioColorMedical, (value) => { SetChannelColor(MedicalChannelProto, value); }, true);
        _config.OnValueChanged(DCCVars.RadioColorEngineering, (value) => { SetChannelColor(EngineeringChannelProto, value); }, true);
        _config.OnValueChanged(DCCVars.RadioColorScience, (value) => { SetChannelColor(ScienceChannelProto, value); }, true);
        _config.OnValueChanged(DCCVars.RadioColorCargo, (value) => { SetChannelColor(CargoChannelProto, value); }, true);
        _config.OnValueChanged(DCCVars.RadioColorService, (value) => { SetChannelColor(ServiceChannelProto, value); }, true);
        _config.OnValueChanged(DCCVars.RadioColorBinary, (value) => { SetChannelColor(BinaryChannelProto, value); }, true);
        _config.OnValueChanged(DCCVars.RadioColorJustice, (value) => { SetChannelColor(JusticeChannelProto, value); }, true);
        _config.OnValueChanged(DCCVars.RadioColorPrison, (value) => { SetChannelColor(PrisonChannelProto, value); }, true);
    }

    private void SetChannelColor(ProtoId<RadioChannelPrototype> channel, string newColor)
    {
        if (Color.TryParse(newColor, out var color))
            _proto.Index(channel).Color = color;
    }
}
