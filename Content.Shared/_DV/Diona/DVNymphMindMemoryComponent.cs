using Robust.Shared.GameStates;

namespace Content.Shared._DV.Diona;

[RegisterComponent, NetworkedComponent]
public sealed partial class DVNymphMindMemoryComponent : Component
{
    [DataField(serverOnly: true)]
    public EntityUid? Mind;
}
