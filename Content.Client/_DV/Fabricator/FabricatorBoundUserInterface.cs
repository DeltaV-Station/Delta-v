using Content.Shared._DV.Fabricator;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._DV.Fabricator;

[UsedImplicitly]
public sealed class FabricatorBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    [ViewVariables]
    private FabricatorMenu? _menu;

    [ViewVariables]
    private Dictionary<int, EntityUid> _solids = new();

    protected override void Open()
    {
        base.Open();
        _menu = this.CreateWindow<FabricatorMenu>();

        _menu.StartButton.OnPressed += _ => SendPredictedMessage(new FabricatorStartMessage());
        _menu.EjectButton.OnPressed += _ => SendPredictedMessage(new FabricatorEjectMessage());

        _menu.SolidsList.OnItemSelected += args =>
            SendPredictedMessage(new FabricatorEjectIndexedMessage(EntMan.GetNetEntity(_solids[args.ItemIndex])));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not FabricatorUpdateUserInterfaceState cState || _menu == null)
            return;

        RefreshContents(cState.ContainedSolids, cState.ContainedReagents);

        _menu.ToggleBusy(cState.Busy);
        _menu.StartButton.Disabled = cState.Busy || cState.ContainedSolids.Length == 0;
        _menu.EjectButton.Disabled = cState.Busy || cState.ContainedSolids.Length == 0;

        _menu.TimeEnd = cState.EndTime;
    }

    private void RefreshContents((NetEntity, int)[] solids, Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> reagents)
    {
        if (_menu is null)
            return;

        _solids.Clear();
        _menu.SolidsList.Clear();
        foreach (var (solidNet, stackSize) in solids)
        {
            var solidEnt = EntMan.GetEntity(solidNet);

            if (EntMan.Deleted(solidEnt))
                return;

            Texture? texture;
            if (EntMan.TryGetComponent<IconComponent>(solidEnt, out var iconComponent))
            {
                texture = EntMan.System<SpriteSystem>().GetIcon(iconComponent);
            }
            else if (EntMan.TryGetComponent<SpriteComponent>(solidEnt, out var spriteComponent))
            {
                texture = spriteComponent.Icon?.Default;
            }
            else
            {
                continue;
            }

            var formatString = EntMan.GetComponent<MetaDataComponent>(solidEnt).EntityName;
            if (stackSize > 1)
                formatString += $" ({stackSize})";

            var item = _menu.SolidsList.AddItem(formatString, texture);
            _solids.Add(_menu.SolidsList.IndexOf(item), solidEnt);
        }

        _menu.ReagentsList.Clear();
        foreach (var (solution, units) in reagents)
        {
            // Don't need to track reagents because they're never ejected

            // get the actual proto from the id
            if (!_prototypeManager.Resolve(solution.Id, out ReagentPrototype? prototype))
                continue;

            var formatString = $"{prototype.LocalizedName} ({units}u)";
            _menu.ReagentsList.AddItem(formatString);
        }
    }
}
