using Content.Client._DV.Light.Controls;
using Content.Client.Items;
using Content.Shared.Light.Components;

namespace Content.Client._DV.Light.EntitySystems;

/// <summary>
/// Handles the label on the light replacer
/// </summary>
public sealed class LightReplacerStatusControlSystem : EntitySystem
{
    public override void Initialize()
    {
        Subs.ItemStatus<Shared._DV.Light.Components.DVLightReplacerComponent>(replacer => new LightReplacerStatusControl(replacer));
    }
}
