using System.Numerics;
using Content.Shared._DV.SignLanguage.Prototypes;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.ResourceManagement;

namespace Content.Client._DV.UserInterfaces.Systems.SignLanguage;

/// <summary>
/// Displays a real-time preview of the sign language construction at the mouse cursor.
/// Updates as the player makes selections through the radial menus.
/// </summary>
public sealed class SignLanguagePreviewOverlay : Overlay
{
    [Dependency] private readonly IInputManager _input = default!;
    [Dependency] private readonly IResourceCache _cache = default!;

    private SignTopicPrototype? _topic;
    private SignEventPrototype? _event;
    private SignIntentPrototype? _intent;
    private SignIntensityPrototype? _intensity;
    private readonly Font _font;
    private readonly Font _helperFont;

    private const int OffsetX = 20;
    private const int OffsetY = 20;
    private const int Padding = 8;
    private const int BackgroundAlpha = 200;

    public SignLanguagePreviewOverlay()
    {
        IoCManager.InjectDependencies(this);
        _font = new VectorFont(_cache.GetResource<FontResource>("/EngineFonts/NotoSans/NotoSans-Regular.ttf"), 14);
        _helperFont = new VectorFont(_cache.GetResource<FontResource>("/EngineFonts/NotoSans/NotoSans-Regular.ttf"), 10);
        ZIndex = 1000; // has to go on top yes
    }

    /// <summary>
    /// Updates the preview with the current selections.
    /// </summary>
    public void UpdatePreview(
        SignTopicPrototype? topic,
        SignEventPrototype? eventProto,
        SignIntentPrototype? intent,
        SignIntensityPrototype? intensity)
    {
        _topic = topic;
        _event = eventProto;
        _intent = intent;
        _intensity = intensity;
    }

    /// <summary>
    /// Clears the preview.
    /// </summary>
    public void ClearPreview()
    {
        _topic = null;
        _event = null;
        _intent = null;
        _intensity = null;
    }

    /// <summary>
    /// Checks if the preview has any content to display.
    /// </summary>
    public bool HasContent()
    {
        return _topic != null;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        // Don't draw if there's no content
        if (!HasContent())
            return;

        var handle = args.ScreenHandle;

        // Get mouse position
        var mousePos = _input.MouseScreenPosition.Position;
        var drawPos = mousePos + new Vector2(OffsetX, OffsetY);

        // Build the preview text
        var previewText = BuildPreviewText();

        // Measure the text
        var textDimensions = handle.GetDimensions(_font, previewText, 1f);

        // Draw background
        var bgRect = new UIBox2(
            drawPos - new Vector2(Padding, Padding),
            drawPos + textDimensions + new Vector2(Padding, Padding)
        );

        handle.DrawRect(bgRect, Color.Black.WithAlpha(BackgroundAlpha));
        handle.DrawRect(bgRect, Color.White.WithAlpha(50), filled: false);

        // Draw text based on completion state
        var textColor = GetTextColor();
        handle.DrawString(_font, drawPos, previewText, textColor);

        // Draw helper text below
        var helperText = GetHelperText();
        var helperPos = drawPos + new Vector2(0, textDimensions.Y + 4);
        handle.DrawString(_helperFont, helperPos, helperText, Color.Gray);
    }

    private string BuildPreviewText()
    {
        var parts = new List<string>();

        if (_topic != null)
            parts.Add(Loc.GetString(_topic.Name));
        else
            parts.Add("???");

        if (_event != null)
            parts.Add(Loc.GetString(_event.Name));
        else
            parts.Add("???");

        if (_intent != null)
            parts.Add(Loc.GetString(_intent.Name));
        else
            parts.Add("???");

        var text = string.Join(" — ", parts);

        // Add intensity formatting if present
        if (_intensity != null && !string.IsNullOrEmpty(_intensity.TextFormatting))
        {
            text += _intensity.TextFormatting;
        }

        return text;
    }

    private Color GetTextColor()
    {
        // Color progression based on completion
        if (_intent != null)
            return Color.LimeGreen; // Complete
        else if (_event != null)
            return Color.Yellow; // Partial
        else if (_topic != null)
            return Color.Orange; // Just started
        else
            return Color.Gray; // No selections
    }

    private string GetHelperText()
    {
        if (_intent != null)
            return Loc.GetString("sign-preview-complete"); // "Press ENTER to send, ESC to cancel"
        else if (_event != null)
            return Loc.GetString("sign-preview-select-intent"); // "Select an intent..."
        else if (_topic != null)
            return Loc.GetString("sign-preview-select-event"); // "Select what's happening..."
        else
            return Loc.GetString("sign-preview-select-topic"); // "Select a topic..."
    }
}
