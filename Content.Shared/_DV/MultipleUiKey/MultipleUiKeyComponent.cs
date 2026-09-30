using Robust.Shared.Network;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._DV.MultipleUiKey;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class MultipleUiKeyComponent : Component
{
    [DataDefinition]
    public sealed partial class ModeEntry
    {
        [DataField(required: true)]
        public Enum UiKey;

        [DataField(required: true)]
        public LocId Name;
    }

    [DataField(required: true)]
    public List<ModeEntry> Modes = new();

    [DataField]
    public bool ShowCurrentMode = true;

    [DataField]
    [AutoNetworkedField]
    public int Index = 0;

    [DataField]
    public string CycleVerbImgResPath = "/Textures/Interface/VerbIcons/refresh.svg.192dpi.png";

    [DataField]
    public string CurrentIndexImgResPath = "/Textures/_DV/Interface/VerbIcons/index.svg.192dpi.png";

    [DataField]
    public LocId VerbCategoryText = "multipleuikey-verb-categories-change-mode";

    [DataField]
    public LocId VerbNextText = "multipleuikey-verb-text-next-mode";

    [DataField]
    public LocId PopUpMessage = "multipleuikey-swap-mode-popup-message-text";

    [DataField]
    public int Priority = 2;

    [ViewVariables(VVAccess.ReadWrite)]
    public string CurrentModeName = string.Empty;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool LabelUpdateNeeded = false;
}
