using Content.Shared.CrewManifest;
using Content.Shared.StatusIcon;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using System.Numerics;
using Content.Shared.Roles;

namespace Content.Client.CrewManifest.UI;

public sealed class CrewManifestSection : BoxContainer
{
    public CrewManifestSection(
        IPrototypeManager prototypeManager,
        SpriteSystem spriteSystem,
        DepartmentPrototype section,
        List<CrewManifestEntry> entries)
    {
        Orientation = LayoutOrientation.Vertical;
        HorizontalExpand = true;

        AddChild(new Label()
        {
            StyleClasses = { "LabelBig" },
            Text = Loc.GetString($"department-{section.ID}")
        });

        // Delta-V - changed type from GridContainer to BoxContainer to better handle long names and titles.
        var gridContainer = new BoxContainer()
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalExpand = true,
        };

        // Delta-V - Start of column BoxContainers.
        var namesContainer = new BoxContainer()
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
            SizeFlagsStretchRatio = 3,
        };

        var titlesContainer = new BoxContainer()
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
            SizeFlagsStretchRatio = 2,
        };

        gridContainer.AddChild(namesContainer);
        gridContainer.AddChild(titlesContainer);
        // Delta-V - end of column BoxContainers.

        AddChild(gridContainer);

        foreach (var entry in entries)
        {
            // Delta-V - start of name and pronoun container
            var nameContainer = new BoxContainer()
            {
                Orientation = LayoutOrientation.Horizontal,
                HorizontalExpand = true,
            };

            var name = new RichTextLabel();
            name.SetMessage(entry.Name);

            var gender = new RichTextLabel()
            {
                Margin = new Thickness(6, 0, 0, 0),
                StyleClasses = { "CrewManifestGender" }
            };
            gender.SetMessage(Loc.GetString("gender-display", ("gender", entry.Gender)));

            nameContainer.AddChild(name);
            nameContainer.AddChild(gender);
            // Delta-V - end of name and pronoun container

            var titleContainer = new BoxContainer()
            {
                Orientation = LayoutOrientation.Horizontal,
                HorizontalExpand = true,
                SizeFlagsStretchRatio = 1, // Delta-V
            };

            var title = new RichTextLabel();
            title.SetMessage(entry.JobTitle);


            if (prototypeManager.TryIndex<JobIconPrototype>(entry.JobIcon, out var jobIcon))
            {
                var icon = new TextureRect()
                {
                    TextureScale = new Vector2(2, 2),
                    VerticalAlignment = VAlignment.Center,
                    Texture = spriteSystem.Frame0(jobIcon.Icon),
                    Margin = new Thickness(0, 0, 4, 0)
                };

                titleContainer.AddChild(icon);
                titleContainer.AddChild(title);
            }
            else
            {
                titleContainer.AddChild(title);
            }

            // Delta-V - grid was replaced with two BoxContainer columns
            namesContainer.AddChild(nameContainer);
            titlesContainer.AddChild(titleContainer);
            // Delta-V - end of grid container change
        }
    }
}
