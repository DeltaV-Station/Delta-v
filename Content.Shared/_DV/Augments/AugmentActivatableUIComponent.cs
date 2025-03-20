using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Shared._DV.Augments;

/// <summary>
///     Component that allows an augment to have a user interface
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AugmentActivatableUIComponent : Component
{
    [DataField(required: true, customTypeSerializer: typeof(EnumSerializer))]
    public Enum? Key;

    [DataField]
    public EntProtoId OpenAction = "ActionOpenAugmentInterface";

    [DataField, AutoNetworkedField]
    public EntityUid? OpenActionEntity;
}

/// <summary>
///     Event that should be dispatched by the <see cref="AugmentActivatableUIComponent.OpenAction"/> to open the UI
/// </summary>
public sealed partial class AugmentUIOpenEvent : InstantActionEvent;
