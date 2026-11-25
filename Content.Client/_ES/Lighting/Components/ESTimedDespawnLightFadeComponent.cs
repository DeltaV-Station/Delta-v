using Content.Shared._ES.Core.Timer.Components;

namespace Content.Client._ES.Lighting.Components;

/// <summary>
/// Handles a point light that fades out while synced to a <see cref="ESTimedDespawnComponent"/>
/// </summary>
[RegisterComponent]
[Access(typeof(ESTimedDespawnLightFadeSystem))]
public sealed partial class ESTimedDespawnLightFadeComponent : Component
{
    [DataField]
    public TimeSpan FadeTime = TimeSpan.FromSeconds(1);
}
