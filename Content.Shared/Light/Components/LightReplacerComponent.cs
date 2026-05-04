using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Storage;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Utility; // DeltaV - Predicted Light Replacer & Added Radial Menu

namespace Content.Shared.Light.Components;

/// <summary>
///     Device that allows user to quikly change bulbs in <see cref="PoweredLightComponent"/>
///     Can be reloaded by new light tubes or light bulbs
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState] // DeltaV - Predicted Light Replacer & Added Radial Menu
[Access(typeof(SharedLightReplacerSystem))]
public sealed partial class LightReplacerComponent : Component
{
    [DataField("sound")]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Weapons/click.ogg")
    {
        Params = new()
        {
            Volume = -4f
        }
    };

    /// <summary>
    /// Bulbs that were inserted inside light replacer
    /// </summary>
    [ViewVariables]
    public Container InsertedBulbs = default!;

    /// <summary>
    /// The default starting bulbs
    /// </summary>
    [DataField("contents")]
    public List<EntitySpawnEntry> Contents = new();

    // Start DeltaV - Predicting Light Replacers & Adding Radial Menu
    /// <summary>
    /// This string defines what kind of tube will be inserted into light fixtures.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ActiveLightTube = "fluorescent light tube";

    /// <summary>
    /// This string defines what kind of bulb will be inserted into light fixtures.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ActiveLightBulb = "incandescent light bulb";

    /// <summary>
    /// The Icon Sprite for ejecting tubes in the radial menu.
    /// </summary>
    [DataField]
    public SpriteSpecifier EjectTubes = new SpriteSpecifier.Rsi(new ResPath("_DV/Objects/Specific/Janitorial/light_replacer.rsi"), "eject-tubes");

    /// <summary>
    /// The Icon Sprite for ejecting bulbs in the radial menu.
    /// </summary>
    [DataField]
    public SpriteSpecifier EjectBulbs = new SpriteSpecifier.Rsi(new ResPath("_DV/Objects/Specific/Janitorial/light_replacer.rsi"), "eject-bulbs");

    // End DeltaV - Predicting Light Replacers & Adding Radial Menu
}
