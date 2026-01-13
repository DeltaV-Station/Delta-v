using Content.Server.Speech.EntitySystems;

namespace Content.Server.Speech.Components;

/// <summary>
/// Anglish tung, the component replaces some words with French, Latin, and other etymologies with Germanic and Norse variations.
/// </summary>
[RegisterComponent]
[Access(typeof(AnglishAccentSystem))]
public sealed partial class AnglishAccentComponent : Component;
