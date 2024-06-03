using Content.Shared.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.ReverseEngineering;

/// <summary>
/// This item has some value in reverse engineering lathe recipes.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedReverseEngineeringSystem))]
[AutoGenerateComponentState]
public sealed partial class ReverseEngineeringComponent : Component
{
    /// <summary>
    /// The recipes that can be reverse engineered from this.
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<LatheRecipePrototype>> Recipes = new();

    /// <summary>
    /// Difficulty score 1-5 how hard this is to reverse engineer.
    /// Rolls have this number taken away from them.
    /// </summary>
    [DataField]
    public int Difficulty = 1;

    /// <summary>
    /// A new item that should be given back by the reverse engineering machine instead of this one.
    /// E.g., NT aligned versions of syndicate items.
    /// </summary>
    [DataField]
    public EntProtoId? NewItem;

    /// <summary>
    /// How far along this specific item has been reverse engineered.
    /// Lets you resume if ejected, after completion it gets reset.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Progress;

    /// <summary>
    /// On client, the message shown in the scan information box.
    /// </summary>
    public FormattedMessage ScanMessage = new();
}
