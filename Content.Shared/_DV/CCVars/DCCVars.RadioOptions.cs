using Robust.Shared.Audio;
using Robust.Shared.Configuration;

namespace Content.Shared._DV.CCVars;

public sealed partial class DCCVars
{
    public static readonly CVarDef<string> RadioColorCommon =
        CVarDef.Create("deltav.options.radio.common_color",
            "#2cdb2c",
            CVar.CLIENTONLY | CVar.ARCHIVE,
            "The color of the Common radio channel.");

    public static readonly CVarDef<string> RadioColorCommand =
        CVarDef.Create("deltav.options.radio.command_color",
            "#fcdf03",
            CVar.CLIENTONLY | CVar.ARCHIVE,
            "The color of the Command radio channel.");

    public static readonly CVarDef<string> RadioColorSecurity =
        CVarDef.Create("deltav.options.radio.security_color",
            "#8c93f5",
            CVar.CLIENTONLY | CVar.ARCHIVE,
            "The color of the Security radio channel.");

    public static readonly CVarDef<string> RadioColorMedical =
        CVarDef.Create("deltav.options.radio.medical_color",
            "#57b8f0",
            CVar.CLIENTONLY | CVar.ARCHIVE,
            "The color of the Medical radio channel.");

    public static readonly CVarDef<string> RadioColorEngineering =
        CVarDef.Create("deltav.options.radio.engineering_color",
            "#ff733c",
            CVar.CLIENTONLY | CVar.ARCHIVE,
            "The color of the Engineering radio channel.");

    public static readonly CVarDef<string> RadioColorScience =
        CVarDef.Create("deltav.options.radio.science_color",
            "#cd7ccd",
            CVar.CLIENTONLY | CVar.ARCHIVE,
            "The color of the Science radio channel.");

    public static readonly CVarDef<string> RadioColorCargo =
        CVarDef.Create("deltav.options.radio.cargo_color",
            "#b48b57",
            CVar.CLIENTONLY | CVar.ARCHIVE,
            "The color of the Cargo radio channel.");

    public static readonly CVarDef<string> RadioColorService =
        CVarDef.Create("deltav.options.radio.service_color",
            "#539c00",
            CVar.CLIENTONLY | CVar.ARCHIVE,
            "The color of the Service radio channel.");

    public static readonly CVarDef<string> RadioColorBinary =
        CVarDef.Create("deltav.options.radio.binary_color",
            "#5ed7aa",
            CVar.CLIENTONLY | CVar.ARCHIVE,
            "The color of the Binary radio channel.");

    public static readonly CVarDef<string> RadioColorJustice =
        CVarDef.Create("deltav.options.radio.justice_color",
            "#c70667",
            CVar.CLIENTONLY | CVar.ARCHIVE,
            "The color of the Justice radio channel.");

    public static readonly CVarDef<string> RadioColorPrison =
        CVarDef.Create("deltav.options.radio.prison_color",
            "#FFA500",
            CVar.CLIENTONLY | CVar.ARCHIVE,
            "The color of the Prison radio channel.");
}
