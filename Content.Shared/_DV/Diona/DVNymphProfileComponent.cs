using Content.Shared.Body;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Enums;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Diona;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DVNymphProfileComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<OrganCategoryPrototype>, OrganProfileData>? OrganProfiles;

    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<OrganCategoryPrototype>, Dictionary<HumanoidVisualLayers, List<Marking>>>? OrganMarkings;

    [DataField, AutoNetworkedField]
    public string? Name;

    [DataField, AutoNetworkedField]
    public ProtoId<SpeciesPrototype> Species = "Diona";

	[DataField, AutoNetworkedField]
    public Gender Gender;

    [DataField, AutoNetworkedField]
    public Sex Sex;

    [DataField, AutoNetworkedField]
    public int Age = 18;

    [DataField, AutoNetworkedField]
    public float Height = 1f;
}
