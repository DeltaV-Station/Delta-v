using Content.Shared._ES.Core.Timer.Components;

namespace Content.Client._ES.Sprite.Components;

/// <summary>
/// Handles a sprite that fades out while synced to a <see cref="ESTimedDespawnComponent"/>
/// </summary>
[RegisterComponent]
[Access(typeof(ESTimedDespawnSpriteFadeSystem))]
public sealed partial class ESTimedDespawnSpriteFadeComponent : Component
{
    [DataField]
    public TimeSpan FadeTime = TimeSpan.FromSeconds(1);
}
