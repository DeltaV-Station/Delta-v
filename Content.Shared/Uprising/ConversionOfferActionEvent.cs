using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.Uprising;

public sealed partial class ConversionOfferActionEvent : EntityTargetActionEvent
{
    [DataField]
    public List<Type> IncompatibleMindRoleTypes;

    [DataField]
    public EntProtoId AcceptAction;
}
