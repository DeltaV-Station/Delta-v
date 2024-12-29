using Robust.Shared.Prototypes;

namespace Content.Shared.DeltaV.FeedbackOverwatch;

/// <summary>
///     Prototype that describes the contents of a feedback popup.
/// </summary>
[Prototype]
public sealed partial class FeedbackPopupPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    ///     Title of the popup. This supports rich text so you can use colors and stuff.
    /// </summary>
    [DataField(required: true)]
    public string Title = "";

    /// <summary>
    ///     List of "paragraphs" that are placed in the middle of the popup. Put the any relavent information about
    ///     what to give feedback on here!
    /// </summary>
    [DataField(required: true)]
    public List<string> Description = new();

    /// <summary>
    ///     Describe where you want to put the feedback here.
    /// </summary>
    [DataField]
    public string? FeedbackLocation;

    /// <summary>
    ///     Link to the discord channel that will be linked when the button is clicked.
    /// </summary>
    /// <remarks>
    ///     Must start with "https://discord.com/".
    /// </remarks>
    [DataField]
    public string? DiscordLink;
}
