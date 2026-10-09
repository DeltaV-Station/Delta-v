using Content.Shared.Actions;
using Content.Shared.Roles.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.Uprising;

public sealed partial class ConversionAcceptActionEvent : InstantActionEvent
{
    [DataField(required: true)]
    public EntProtoId MindRole;

    [DataField(required: true)]
    public EntProtoId<MindRoleComponent> RemoveMindRole;
}
