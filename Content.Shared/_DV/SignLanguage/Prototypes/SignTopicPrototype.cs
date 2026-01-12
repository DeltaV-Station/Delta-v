using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._DV.SignLanguage.Prototypes;

/// <summary>
/// Sign language topic - first menu in the sign language system.
/// Represents what the sign is about (People, Locations, Objects, General).
/// </summary>
[Prototype]
public sealed class SignTopicPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Localization string for the topic name. Displayed in the radial UI.
    /// </summary>
    [DataField(required: true)]
    public LocId Name;

    /// <summary>
    /// Category this topic belongs to. Determines which events are available in Menu 2.
    /// </summary>
    [DataField(required: true)]
    public SignTopicCategory Category;

    /// <summary>
    /// An icon used to visually represent the topic in radial UI.
    /// </summary>
    [DataField]
    public SpriteSpecifier Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Actions/sign.png"));

    /// <summary>
    /// Sort order within the category for consistent layout.
    /// </summary>
    [DataField]
    public int Priority;
}

/// <summary>
/// Categories for sign language topics. Determines which events are contextually available.
/// </summary>
[Flags]
[Serializable] [NetSerializable]
public enum SignTopicCategory : byte
{
    Invalid = 0,
    People = 1 << 0,
    Locations = 1 << 1,
    Objects = 1 << 2,
    General = 1 << 3,
    All = byte.MaxValue,
}
