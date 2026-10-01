using Robust.Shared.Map.Components;
using Robust.Shared.Utility;
using Content.Shared.Verbs;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Shared._DV.MultipleUiKey;

public abstract partial class SharedMultipleUiKeySystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MultipleUiKeyComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<MultipleUiKeyComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
        return;
    }

    private void OnComponentStartup(EntityUid uid, MultipleUiKeyComponent comp, ComponentStartup args)
    {
        if (comp.Index >= 0 && comp.Index < comp.Modes.Count)
            comp.CurrentModeName = Loc.GetString(comp.Modes.ElementAt(comp.Index).Name);
        return;
    }

    private void AddAlternativeVerbs(EntityUid uid, MultipleUiKeyComponent comp, GetVerbsEvent<AlternativeVerb> args)
    {
        // If there are fewer than two modes,
        // we can't swap between anything
        if (comp.Modes.Count < 2)
            return;

        VerbCategory category = new(Loc.GetString(comp.VerbCategoryText), comp.CycleVerbImgResPath);

        AlternativeVerb verb = new()
        {
            Text = Loc.GetString(comp.VerbNextText),
            Act = () => NextUiKey(uid, comp, args.User),
            Priority = comp.Priority,
            Category = category
        };

        args.Verbs.Add(verb);

        foreach (MultipleUiKeyComponent.ModeEntry mode in comp.Modes)
        {
            verb = new()
            {
                // Unfortunately Verbs are sorted by name if they don't have a priority
                // so we need to prepend the index (convert to 1-index)
                Text = String.Format("{0}. {1}", comp.Modes.IndexOf(mode) + 1, Loc.GetString(mode.Name)),
                Act = () => SelectUiKey(uid, comp, args.User, comp.Modes.IndexOf(mode)),
                Category = category
            };

            if (comp.Index == comp.Modes.IndexOf(mode))
            {
                verb.Icon = new SpriteSpecifier.Texture(new(comp.CurrentIndexImgResPath));
            }

            args.Verbs.Add(verb);
        }

        return;
    }

    private void NextUiKey(EntityUid uid, MultipleUiKeyComponent comp, EntityUid user)
    {
        // If there are fewer than 1 modes, we shouldn't have gotten
        // here but there's nothing to swap to, do nothing
        // Protects against mod by 0 below
        if (comp.Modes.Count < 1)
            return;

        SelectUiKey(uid, comp, user, (comp.Index + 1) % comp.Modes.Count);

        return;
    }

    private void SelectUiKey(EntityUid uid, MultipleUiKeyComponent comp, EntityUid user, int index)
    {
        // Shouldn't happen, but better to check than crash the server
        if (index < 0 || index >= comp.Modes.Count)
            return;

        comp.Index = index;

        // Dirty the component so that both client+server will see the new index
        Dirty(uid, comp);
        SetUiKey(uid, comp, user);

        return;
    }

    public virtual void SetUiKey(EntityUid uid, MultipleUiKeyComponent comp, EntityUid user)
    {
        if (!_entManager.TryGetComponent<ActivatableUIComponent>(uid, out var uiComp))
            return;

        // Close the current UI before updating the key so
        // the user can't have both of them open at once
        if (uiComp.Key != null)
            _uiSystem.CloseUi(uid, uiComp.Key!);

        // Update the ActivatableUI key and the current mode name
        uiComp.Key = comp.Modes.ElementAt(comp.Index).UiKey;
        comp.CurrentModeName = Loc.GetString(comp.Modes.ElementAt(comp.Index).Name);

        return;
    }
}
