using Content.Client.UserInterface.Controls;

Robust.Client.UserInterface.XAML;

namespace Content.Client._DV.CosmicCult.UI;

[GenerateTypedNameReferences]
public sealed partial class CosmicRoundStartMenu : FancyWindow
{
    public CosmicRoundStartMenu()
    {
        RobustXamlLoader.Load(this);

        ConfirmButton.OnPressed += _ => Close();
    }
}
