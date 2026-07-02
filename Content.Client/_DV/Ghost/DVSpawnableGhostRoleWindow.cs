using System.Linq;
using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Ghost.Controls.Roles;
using Content.Shared._DV.Ghost.Roles;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.Ghost;

public sealed class DVSpawnableGhostRoleWindow : FancyWindow
{
    public event Action<ProtoId<DVSpawnableGhostRolePrototype>>? RoleSelected;

    private GhostRoleRulesWindow? _windowRules = null;
    private readonly BoxContainer _entries;
    private readonly IPrototypeManager _prototype;

    public DVSpawnableGhostRoleWindow(IPrototypeManager prototype)
    {
        _prototype = prototype;

        Title = Loc.GetString("ghost-gui-spawn-vent-critter-window-title");
        MinSize = new Vector2(490, 400);
        SetSize = new Vector2(490, 500);

        _entries = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 8,
            Margin = new Thickness(8),
        };

        var scroll = new ScrollContainer
        {
            HScrollEnabled = false,
            Children =
            {
                _entries,
            },
        };

        ContentsContainer.AddChild(scroll);
        Populate();

        OnClose += () => _windowRules?.Close();
    }

    private void Populate()
    {
        var roles = _prototype.EnumeratePrototypes<DVSpawnableGhostRolePrototype>()
            .OrderBy(role => Loc.GetString(role.Name));

        foreach (var role in roles)
        {
            var info = new GhostRoleInfoBox(Loc.GetString(role.Name), Loc.GetString(role.Description));

            var button = new Button
            {
                Text = Loc.GetString("ghost-gui-spawn-vent-critter-spawn-button"),
                VerticalAlignment = VAlignment.Center,
            };

            var id = new ProtoId<DVSpawnableGhostRolePrototype>(role.ID);
            button.OnPressed += _ =>
            {
                _windowRules?.Close();

                _windowRules = new GhostRoleRulesWindow(Loc.GetString(role.Rules),
                    _ =>
                {
                    RoleSelected?.Invoke(id);
                    Close();
                });
                _windowRules.OnClose += () =>
                {
                    _windowRules = null;
                };
                _windowRules.OpenCentered();
            };

            _entries.AddChild(info);
            _entries.AddChild(button);
        }

        if (_entries.ChildCount != 0)
            return;

        _entries.AddChild(new Label
        {
            Text = Loc.GetString("ghost-gui-spawn-vent-critter-none"),
        });
    }
}
