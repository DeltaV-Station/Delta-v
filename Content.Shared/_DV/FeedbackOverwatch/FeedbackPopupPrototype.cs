using Robust.Shared.Prototypes;

namespace Content.Shared._DV.FeedbackOverwatch;

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
    ///     Name of the popup. This is relayed in the discord webhook.
    /// </summary>
    /// <remarks>
    ///     Recommended to keep this one word to make searching easier.
    /// </remarks>
    [DataField(required: true)]
    public LocId PopupName;

    /// <summary>
    ///     Title of the popup. This supports rich text so you can use colors and stuff.
    /// </summary>
    [DataField(required: true)]
    public LocId Title;

    /// <summary>
    ///     List of "paragraphs" that are placed in the middle of the popup. Put any relevant information about what to give feedback on here!
    /// </summary>
    [DataField(required: true)]
    public List<LocId> Description = new();

    /// <summary>
    ///     If true, will show a text field that players can fill out and will be piped through the discord webhook if enabled.
    /// </summary>
    [DataField]
    public bool FeedbackField = true;
}
