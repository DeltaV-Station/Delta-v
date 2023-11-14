using Robust.Client.UserInterface.Controls;

namespace Content.Client.Info
{
    public sealed partial class RulesAndInfoWindow
    {
        private void PopulateSop(TabContainer sopList)
        {
            var sopIntro = new Info();
            var sopAuthority = new Info();
            var sopMutiny = new Info();

            sopList.AddChild(sopIntro);
            sopList.AddChild(sopAuthority);
            sopList.AddChild(sopMutiny);

            TabContainer.SetTabTitle(sopIntro, Loc.GetString("ui-info-tab-intro"));
            TabContainer.SetTabTitle(sopAuthority, Loc.GetString("ui-info-tab-authority"));
            TabContainer.SetTabTitle(sopMutiny, Loc.GetString("ui-info-tab-Mutiny"));

            AddSection(sopIntro, Loc.GetString("ui-info-header-intro"), "DeltaV/SOP/Intro.txt");
            AddSection(sopAuthority, Loc.GetString("ui-info-header-authority"), "DeltaV/SOP/Authority.txt", true);
            AddSection(sopMutiny, Loc.GetString("ui-info-header-Mutiny"), "DeltaV/SOP/Mutiny.txt", true);
        }
    }
}
