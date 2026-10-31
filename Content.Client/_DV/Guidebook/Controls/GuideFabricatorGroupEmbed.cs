using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Client.Guidebook.Richtext;
using Content.Shared._DV.Fabricator;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.Guidebook.Controls;

public sealed partial class GuideFabricatorGroupEmbed : BoxContainer, IDocumentTag
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public GuideFabricatorGroupEmbed()
    {
        Orientation = LayoutOrientation.Vertical;
        IoCManager.InjectDependencies(this);
        MouseFilter = MouseFilterMode.Stop;
    }

    public bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        control = null;
        if (!args.TryGetValue("Group", out var group))
            return false;

        CreateEntries(group);

        control = this;
        return true;
    }

    private void CreateEntries(string group)
    {
        var prototypes = _prototype.EnumeratePrototypes<FabricatorRecipePrototype>()
            .Where(p => p.Group.Equals(group))
            .OrderBy(p => p.Name);

        foreach (var recipe in prototypes)
        {
            AddChild(new GuideFabricatorEmbed(recipe));
        }
    }
}
