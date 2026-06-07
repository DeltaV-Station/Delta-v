using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Server._DV.Silicons.MMI;

[RegisterComponent]
public sealed partial class UnborgableMMIComponent : Component
{
    [DataField("brainSlotId")]
    public string BrainSlotId = "brainslot";
}
