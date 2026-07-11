using System.Diagnostics.CodeAnalysis;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement; //Harmony Lanyards
using Content.Shared.Inventory; //Harmony Lanyards
using Content.Shared.Labels.Components;
using Content.Shared.NameModifier.EntitySystems;
using Content.Shared.Paper;
using Content.Shared.Tag; // DeltaV
using Robust.Shared.Containers;
using Robust.Shared.Prototypes; // DeltaV
using Robust.Shared.Utility;
using System.Linq; //Harmony Lanyards

namespace Content.Shared.Labels.EntitySystems;

public sealed partial class LabelSystem : EntitySystem
{
    [Dependency] private readonly NameModifierSystem _nameModifier = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly TagSystem _tag = default!; // DeltaV

    public const string ContainerName = "paper_label";
    public static readonly ProtoId<TagPrototype> PreventTag = "PreventLabel"; // DeltaV - no labelling felenids

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LabelComponent, MapInitEvent>(OnLabelCompMapInit);
        SubscribeLocalEvent<LabelComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<LabelComponent, RefreshNameModifiersEvent>(OnRefreshNameModifiers);

        SubscribeLocalEvent<PaperLabelComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<PaperLabelComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<PaperLabelComponent, EntInsertedIntoContainerMessage>(OnContainerModified);
        SubscribeLocalEvent<PaperLabelComponent, EntRemovedFromContainerMessage>(OnContainerModified);
        SubscribeLocalEvent<PaperLabelComponent, ExaminedEvent>(OnExamined);
        // Harmony - OnExamined can now be inventory relayed from the neck slot for lanyard implementation,
        // getting the entity's pronouns and changing the label inspection text to reflect it being from a lanyard.
        // This would cause any other neck slot item with the label component to be described as a lanyard on inspection,
        // but currently no others exist.
        SubscribeLocalEvent<PaperLabelComponent, InventoryRelayedEvent<ExaminedEvent>>((e, c, ev) => OnExaminedInInventory(e, c, ev.Args));
    }

    private void OnLabelCompMapInit(Entity<LabelComponent> ent, ref MapInitEvent args)
    {
        if (!string.IsNullOrEmpty(ent.Comp.CurrentLabel))
        {
            ent.Comp.CurrentLabel = Loc.GetString(ent.Comp.CurrentLabel);
            Dirty(ent);
        }

        _nameModifier.RefreshNameModifiers(ent.Owner);
    }

    /// <summary>
    /// Apply or remove a label on an entity.
    /// </summary>
    /// <param name="uid">EntityUid to change label on</param>
    /// <param name="text">intended label text (null to remove)</param>
    /// <param name="label">label component for resolve</param>
    /// <param name="metadata">metadata component for resolve</param>
    public void Label(EntityUid uid, string? text, MetaDataComponent? metadata = null, LabelComponent? label = null)
    {
        if (_tag.HasTag(uid, PreventTag)) // DeltaV - Prevent labelling felinids
            return;
        label ??= EnsureComp<LabelComponent>(uid);

        label.CurrentLabel = text == null ? null : FormattedMessage.EscapeText(text);
        _nameModifier.RefreshNameModifiers(uid);

        Dirty(uid, label);
    }

    private void OnExamine(Entity<LabelComponent> ent, ref ExaminedEvent args)
    {
        if (!ent.Comp.Examinable)
            return;

        if (ent.Comp.CurrentLabel == null)
            return;

        var message = new FormattedMessage();
        message.AddText(Loc.GetString("hand-labeler-has-label", ("label", ent.Comp.CurrentLabel)));
        args.PushMessage(message);
    }

    private void OnRefreshNameModifiers(Entity<LabelComponent> entity, ref RefreshNameModifiersEvent args)
    {
        if (!string.IsNullOrEmpty(entity.Comp.CurrentLabel))
            args.AddModifier("comp-label-format", extraArgs: ("label", entity.Comp.CurrentLabel));
    }

    private void OnComponentInit(Entity<PaperLabelComponent> ent, ref ComponentInit args)
    {
        _itemSlots.AddItemSlot(ent, ContainerName, ent.Comp.LabelSlot);

        UpdateAppearance(ent);
    }

    private void OnComponentRemove(Entity<PaperLabelComponent> ent, ref ComponentRemove args)
    {
        _itemSlots.RemoveItemSlot(ent, ent.Comp.LabelSlot);
    }

    private void OnExamined(Entity<PaperLabelComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.LabelSlot.Item is not {Valid: true} item)
            return;

        using (args.PushGroup(nameof(PaperLabelComponent)))
        {
            if (!args.IsInDetailsRange)
            {
                args.PushMarkup(Loc.GetString("comp-paper-label-has-label-cant-read"));
                return;
            }

            // Assuming yaml has the correct entity whitelist, this should not happen.
            if (!TryComp<PaperComponent>(item, out var paper))
                return;

            if (string.IsNullOrWhiteSpace(paper.Content))
            {
                args.PushMarkup(Loc.GetString("comp-paper-label-has-label-blank"));
                return;
            }

            args.PushMarkup(Loc.GetString("comp-paper-label-has-label"));
            var text = paper.Content;
            args.PushMarkup(text.TrimEnd());
            // Harmony addition begins - shows which stamps have been applied to a label when inspected. Copied from PaperSystem.
            if (paper.StampedBy.Count > 0)
            {
                var commaSeparated =
                    string.Join(", ", paper.StampedBy.Select(s => Loc.GetString(s.StampedName)));
                args.PushMarkup(
                    Loc.GetString(
                        "comp-label-examine-detail-stamped-by",
                        ("stamps", commaSeparated))
                );
            }
            // Harmony addition ends
        }
    }

    // Harmony addition begins - version of OnExamined for when the event is inventory relayed. Used when reading from a worn lanyard.
    private void OnExaminedInInventory(EntityUid uid, PaperLabelComponent comp, ExaminedEvent args)
    {
        if (comp.LabelSlot.Item is not { Valid: true } item)
            return;

        using (args.PushGroup(nameof(PaperLabelComponent)))
        {
            // UID's parent is saved to be used for localisation grammar when the label is from a lanyard, since the text is changed.
            var user = Comp<TransformComponent>(uid).ParentUid;
            if (!args.IsInDetailsRange)
            {
                args.PushMarkup(Loc.GetString("comp-lanyard-has-lanyard-cant-read", ("user", Identity.Entity(user, EntityManager))));
                return;
            }

            if (!TryComp(item, out PaperComponent? paper))
                // Assuming yaml has the correct entity whitelist, this should not happen.
                return;

            if (string.IsNullOrWhiteSpace(paper.Content))
            {
                args.PushMarkup(Loc.GetString("comp-lanyard-has-lanyard-blank", ("user", Identity.Entity(user, EntityManager))));
                return;
            }

            args.PushMarkup(Loc.GetString("comp-lanyard-has-lanyard", ("user", Identity.Entity(user, EntityManager))));
            var text = paper.Content;
            args.PushMarkup(text.TrimEnd());
            // Harmony - shows which stamps have been applied to a lanyard's label when inspected. Copied from PaperSystem.
            if (paper.StampedBy.Count > 0)
            {
                var commaSeparated =
                    string.Join(", ", paper.StampedBy.Select(s => Loc.GetString(s.StampedName)));
                args.PushMarkup(
                    Loc.GetString(
                        "comp-lanyard-examine-detail-stamped-by",
                        ("stamps", commaSeparated))
                );
            }
        }
    }
    // Harmony Addition Ends

    // Not ref-sub due to being used for multiple subscriptions.
    private void OnContainerModified(EntityUid uid, PaperLabelComponent label, ContainerModifiedMessage args)
    {
        if (!label.Initialized)
            return;

        if (args.Container.ID != label.LabelSlot.ID)
            return;

        UpdateAppearance((uid, label));
    }

    private void UpdateAppearance(Entity<PaperLabelComponent, AppearanceComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp2, false))
            return;

        var slot = ent.Comp1.LabelSlot;
        _appearance.SetData(ent, PaperLabelVisuals.HasLabel, slot.HasItem, ent.Comp2);
        if (TryComp<PaperLabelTypeComponent>(slot.Item, out var type))
            _appearance.SetData(ent, PaperLabelVisuals.LabelType, type.PaperType, ent.Comp2);
    }

    /// <summary>
    /// Retrieves a label with the specified component from the default label slot.
    /// </summary>
    public bool TryGetLabel<T>(Entity<PaperLabelComponent?> ent, [NotNullWhen(true)] out Entity<T>? label) where T : Component
    {
        label = null;
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (ent.Comp.LabelSlot.Item is not { } labelEnt)
            return false;

        if (!TryComp<T>(labelEnt, out var labelComp))
            return false;

        label = (labelEnt, labelComp);
        return true;
    }
}
