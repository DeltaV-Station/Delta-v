using Robust.Shared.GameStates;

namespace Content.Shared._DV.Diona;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(DVGestaltSystem))]
public sealed partial class DVGestaltComponent : Component
{
    [DataField(serverOnly: true)]
    public EntityUid NymphStorageMap;

    [DataField(serverOnly: true)]
    public HashSet<EntityUid> StoredNymphs = new();

    [DataField, AutoNetworkedField]
    public int NymphCount;

    [DataField]
    public int RequiredNymphs = 3;

    /// <summary>
    /// The text that appears when attempting to split.
    /// </summary>
    [DataField]
    public LocId PopupText = "diona-gib-action-use";
}
