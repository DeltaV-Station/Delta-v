using System.Numerics;
using System.Threading;
using Content.Shared._DV.Screens;
using Robust.Client.Animations;
using Robust.Client.Graphics;

namespace Content.Client._DV.Screens;

[RegisterComponent]
[Access(typeof(DVTextVisualsSystem), typeof(DVTextRenderingOverlay))]
public sealed partial class DVTextVisualsComponent : Component
{
    [DataField(required: true)]
    public List<DVTextVisualsRow> Rows;

    [DataField]
    public TimeSpan MarqueeRate = TimeSpan.FromSeconds(0.045f);

    [DataField]
    public int MarqueeWidth = 24;

    [DataField]
    public int MarqueePadding = 8;

    public Animation? Animation;

    public CancellationTokenSource? Token;
}

[DataDefinition]
public sealed partial class DVTextVisualsRow
{
    public IRenderTexture? Texture;

    [DataField]
    public string Text;

    [DataField]
    public Vector2 Offset;

    [DataField(required: true)]
    public Enum Layer = DVTextScreenVisualLayers.Line1;
}
