using Content.Client.UserInterface.Controls;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Prototypes;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.CosmicCult.UI;

[UsedImplicitly]
public sealed class CosmicGlyphDrawBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private SimpleRadialMenu? _menu;

    protected override void Open()
    {
        base.Open();

        IoCManager.InjectDependencies(this);

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.OpenOverMouseScreenPosition();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not CosmicGlyphDrawBuiState glyphState || _menu == null)
            return;

        var options = new List<RadialMenuOptionBase>();
        foreach (var glyphId in glyphState.Glyphs)
        {
            var glyph = _prototype.Index(glyphId);

            options.Add(new RadialMenuActionOption<ProtoId<GlyphPrototype>>(SelectGlyph, glyph.ID)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(glyph.Entity),
                ToolTip = $"{Loc.GetString(glyph.Name)}\n{Loc.GetString(glyph.Tooltip)}",
            });
        }

        _menu.SetButtons(options);
    }

    private void SelectGlyph(ProtoId<GlyphPrototype> glyphId)
    {
        SendMessage(new CosmicGlyphDrawSelectedMessage(glyphId));
    }
}
