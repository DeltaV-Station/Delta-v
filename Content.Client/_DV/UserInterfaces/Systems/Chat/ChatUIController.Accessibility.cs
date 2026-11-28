using Content.Client.UserInterface.Systems.Chat.Widgets;
using Content.Shared._DV.CCVars;

namespace Content.Client.UserInterface.Systems.Chat;

public sealed partial class ChatUIController
{
    /// <summary>
    ///     Cached dictionary mapping input colors into safe colors, for accessibility
    /// </summary>
    /// <remarks>
    ///     This should only need to be cleared if the actual background color (not alpha) ever changes... which it doesn't right now.
    /// </remarks>
    private Dictionary<Color, Color> _safeColors = [];

    private bool _forceChatContrast;

    private void InitializeAccessibility()
    {
        _config.OnValueChanged(DCCVars.ForceChatContrast, value => { _forceChatContrast = value; });
        _forceChatContrast = _config.GetCVar(DCCVars.ForceChatContrast);
    }

    /// <summary>
    ///     Get the relative luminance of the specified color.
    /// </summary>
    /// <remarks>
    ///     <para>See https://www.w3.org/WAI/GL/wiki/Relative_luminance</para>
    /// </remarks>
    /// <param name="color"></param>
    /// <returns></returns>
    private float GetRelativeLuminance(Color color)
    {
        var linear = Color.FromSrgb(color);

        return linear.R * 0.2126f + linear.G * 0.7152f + linear.B * 0.0722f;
    }

    /// <summary>
    ///     Get the contrast ratio between a given foreground and background.
    /// </summary>
    /// <remarks>
    ///     <para>See https://www.w3.org/WAI/GL/wiki/Contrast_ratio</para>
    /// </remarks>
    /// <param name="foreground"></param>
    /// <param name="background"></param>
    /// <returns></returns>
    private float GetContrastRatio(Color foreground, Color background)
    {
        var fgLum = GetRelativeLuminance(foreground);
        var bgLum = GetRelativeLuminance(background);

        return (fgLum + 0.05f) / (bgLum + 0.05f);
    }

    /// <summary>
    ///     Get the background color of the chatbox.
    /// </summary>
    /// <param name="foreground">The desired foreground color</param>
    /// <param name="chatBox">Target chat box, or whatever the UI manager determines to be the chat box if null.</param>
    /// <param name="minContrast">
    ///     Target WCAG contrast value.
    ///     <b>4.5</b> is considered "minimum contrast" (or "AA compliance"). <b>7</b> is considered "AAA compliance".
    /// </param>
    /// <returns></returns>
    public Color GetSafeForegroundColor(Color foreground, ChatBox? chatBox = null, float minContrast = 4.5f)
    {
        if (!_forceChatContrast)
            return foreground;

        // check cache
        if (_safeColors.TryGetValue(foreground, out var safe))
            return safe;

        var background = GetChatBackgroundColor(chatBox);

        // check if we're already good
        if (GetContrastRatio(foreground, background) >= minContrast)
            return _safeColors[foreground] = foreground;

        // figure out which way to calculate
        var target = GetContrastRatio(Color.Black, background) >= GetContrastRatio(Color.White, background)
            ? Color.Black
            : Color.White;

        // find something with a decent contrast.
        float low = 0f,
              high = 1f;

        var bestColor = foreground;

        for (var i = 0; i < 5; i++) // Do a binary search to get decently "close" to minContrast.
        {
            var blend = (low + high) / 2.0f;

            var candidate = Color.InterpolateBetween(foreground, target, blend);

            if (GetContrastRatio(candidate, background) >= minContrast)
            {
                bestColor = candidate;
                high = blend;
            }
            else
            {
                low = blend;
            }
        }

        return _safeColors[foreground] = bestColor;
    }
}
