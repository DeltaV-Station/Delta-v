using System.Linq; // DeltaV
using Content.Client.Pinpointer.UI;
using Content.Client.Station; // DeltaV
using Robust.Client.Graphics;
using Robust.Client.Player; // DeltaV
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Client.Medical.CrewMonitoring;

public sealed partial class CrewMonitoringNavMapControl : NavMapControl
{
    [Dependency] private IPlayerManager _playerManager = default!; // DeltaV
    private readonly SharedTransformSystem _xform; // DeltaV
    private readonly StationSystem _station; // DeltaV

    public NetEntity? Focus;
    public Dictionary<NetEntity, string> LocalizedNames = new();

    private Label _trackedEntityLabel;
    private PanelContainer _trackedEntityPanel;

    public CrewMonitoringNavMapControl() : base()
    {
        IoCManager.InjectDependencies(this); // DeltaV - Gotta inject!
        _xform = EntManager.System<SharedTransformSystem>(); // DeltaV - Gotta inject!
        _station = EntManager.System<StationSystem>(); // DeltaV - Gotta Inject!

        WallColor = new Color(192, 122, 196);
        TileColor = new(71, 42, 72);
        BackgroundColor = Color.FromSrgb(TileColor.WithAlpha(BackgroundOpacity));

        _trackedEntityLabel = new Label
        {
            Margin = new Thickness(10f, 8f),
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            Modulate = Color.White,
        };

        _trackedEntityPanel = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = BackgroundColor,
            },

            Margin = new Thickness(5f, 10f),
            HorizontalAlignment = HAlignment.Left,
            VerticalAlignment = VAlignment.Bottom,
            Visible = false,
        };

        _trackedEntityPanel.AddChild(_trackedEntityLabel);
        this.AddChild(_trackedEntityPanel);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (Focus == null)
        {
            _trackedEntityLabel.Text = string.Empty;
            _trackedEntityPanel.Visible = false;

            return;
        }

        foreach ((var netEntity, var blip) in TrackedEntities)
        {
            if (netEntity != Focus)
                continue;

            if (!LocalizedNames.TryGetValue(netEntity, out var name))
                name = Loc.GetString("navmap-unknown-entity");

            var pos = _xform.ToMapCoordinates(blip.Coordinates); // DeltaV - map-coordinates

            var message = name + "\n" + Loc.GetString("navmap-location",
                ("x", MathF.Round(pos.X)), // DeltaV - map-coordinates
                ("y", MathF.Round(pos.Y))); // DeltaV - map-coordinates

            _trackedEntityLabel.Text = message;
            _trackedEntityPanel.Visible = true;

            return;
        }

        _trackedEntityLabel.Text = string.Empty;
        _trackedEntityPanel.Visible = false;
    }

    /// <summary>
    /// DeltaV - Do some things before the base NavMap Draw and also force the redraw of the map
    /// after if the MapUid was null when the CrewMonitor UI was opened.
    /// </summary>
    /// <param name="handle"></param>
    protected override void Draw(DrawingHandleScreen handle)
    {
        // MapUid will not have a value if the crew monitor UI was activated off-grid.
        var forceMapUpdate = false;
        if (!MapUid.HasValue && _playerManager.LocalEntity is { } player)
        {
            MapUid = _xform.GetGrid(player);

            // If it still doesn't have a value, just use the largest grid of the station.
            if (!MapUid.HasValue)
                MapUid = _station.GetLargestGrid(_station.GetStations().First());

            forceMapUpdate = MapUid.HasValue;
        }

        base.Draw(handle);

        // We need to call this after Draw() because UpdateNavMap has functions that uses variables
        // set in Draw()
        if (forceMapUpdate)
            UpdateNavMap();
    }
}
