using Content.Server.Chemistry.Components;
using Robust.Shared.Containers;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Verbs;
using Content.Shared.Examine;
using Content.Server.Popups;
using Content.Shared.Labels.EntitySystems;

namespace Content.Server.Chemistry.EntitySystems;

public sealed partial class ReagentDispenserSystem : EntitySystem
{
    [Dependency] private readonly LabelSystem _label = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    private void InitializeAutoLabeling()
    {
        SubscribeLocalEvent<ReagentDispenserComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<ReagentDispenserComponent, GetVerbsEvent<ExamineVerb>>(OnExamineVerb);
        SubscribeLocalEvent<ReagentDispenserComponent, ExaminedEvent>(OnExamined);
    }

    private void OnEntInserted(Entity<ReagentDispenserComponent> ent, ref EntInsertedIntoContainerMessage ev)
    {
        if (!ent.Comp.CanAutoLabel)
            return;

        if (!ent.Comp.AutoLabelToggle)
            return;

        if (!_solutionContainerSystem.TryGetDrainableSolution(ev.Entity, out _, out var sol))
            return;

        if (sol.GetPrimaryReagentId() is not { } reagentProtoId)
            return;

        if (!_prototypeManager.TryIndex<ReagentPrototype>(reagentProtoId.Prototype, out var reagent))
            return;

        var reagentQuantity = sol.GetReagentQuantity(reagentProtoId);
        var totalQuantity = sol.Volume;
        if (reagentQuantity == totalQuantity)
            _label.Label(ev.Entity, reagent.LocalizedName);
        else
            _label.Label(ev.Entity, Loc.GetString("reagent-dispenser-component-impure-auto-label", ("reagent", reagent.LocalizedName), ("purity", 100.0f * reagentQuantity / totalQuantity)));

        UpdateUiState(ent);
    }

    private void OnExamineVerb(Entity<ReagentDispenserComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!ent.Comp.CanAutoLabel)
            return;

        args.Verbs.Add(new ExamineVerb()
        {
            Act = () =>
            {
                SetAutoLabel(ent, !ent.Comp.AutoLabelToggle);
            },
            Text = ent.Comp.AutoLabelToggle ?
            Loc.GetString("reagent-dispenser-component-set-auto-label-off-verb")
            : Loc.GetString("reagent-dispenser-component-set-auto-label-on-verb"),
            Priority = -1, // Not important, low priority.
            CloseMenu = true
        });
    }

    private void SetAutoLabel(Entity<ReagentDispenserComponent> ent, bool autoLabel)
    {
        if (!ent.Comp.CanAutoLabel)
            return;

        ent.Comp.AutoLabelToggle = autoLabel;

        var popupMessage = autoLabel ? Loc.GetString("reagent-dispenser-component-verb-auto-label-turn-on")
            : Loc.GetString("reagent-dispenser-component-verb-auto-label-turn-off");

        _popup.PopupEntity(popupMessage, ent.Owner);
    }

    private void OnExamined(Entity<ReagentDispenserComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange || !ent.Comp.CanAutoLabel)
            return;

        if (ent.Comp.AutoLabelToggle)
            args.PushMarkup(Loc.GetString("reagent-dispenser-component-examine-auto-label-on"));
        else
            args.PushMarkup(Loc.GetString("reagent-dispenser-component-examine-auto-label-off"));
    }
}
