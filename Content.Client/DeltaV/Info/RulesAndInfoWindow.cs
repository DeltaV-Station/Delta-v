using System.Linq;
using Content.Client.DeltaV.TabbedRules;
using Content.Client.DeltaV.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client.Info
{
    public sealed partial class RulesAndInfoWindow
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        private void PopulateSop(TabbedRules sopList)
        {
            var entries = _prototypeManager.EnumeratePrototypes<TabbedEntryPrototype>()
                .Where(x => x.Container == "SOP")
                .ToDictionary(x => x.ID, x => (TabbedEntry) x);

            sopList.UpdateEntries(entries);
            //sopList.Tree.MaxWidth = 200;
        }
    }
}
