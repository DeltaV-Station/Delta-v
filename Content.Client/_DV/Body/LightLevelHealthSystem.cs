using Content.Client._DV.Light;
using Content.Shared._DV.Body;
using Content.Shared.Alert;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.Body;

public sealed class LightLevelHealthSystem : SharedLightLevelHealthSystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly LightReactiveSystem _lightReactive = default!;

    [ValidatePrototypeId<AlertPrototype>]
    private const string LightLevelDarkIcon = "LightLevelDarkIcon";
    [ValidatePrototypeId<AlertPrototype>]
    private const string LightLevelNeutralIcon = "LightLevelNeutralIcon";
    [ValidatePrototypeId<AlertPrototype>]
    private const string LightLevelBrightIcon = "LightLevelBrightIcon";

    [ValidatePrototypeId<AlertCategoryPrototype>]
    private const string LightAlertCategory = "Light";

    private int _lastThreshold = 0;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // If we're a LightLevelHealthComponent client, update our alerts.
        if (_player.LocalSession?.AttachedEntity is null)
            return;
        var ent = _player.LocalSession.AttachedEntity.Value;

        if (!TryComp<LightLevelHealthComponent>(ent, out var lightLevelHealth))
            return;

        var currentThreshold = CurrentThreshold(_lightReactive.GetLightLevelForPoint(ent), lightLevelHealth);
        var alertIcon = currentThreshold switch
        {
            -1 => LightLevelDarkIcon,
            1 => LightLevelBrightIcon,
            _ => LightLevelNeutralIcon,
        };

        if (currentThreshold != _lastThreshold)
        {
            if (_prototype.TryIndex<AlertCategoryPrototype>(LightAlertCategory, out var alertCategory))
                _alerts.ClearAlertCategory(ent, alertCategory);
        }
        _lastThreshold = currentThreshold;

        if (!_prototype.TryIndex<AlertPrototype>(alertIcon, out var alertProto))
        {
            return;
        }

        _alerts.ShowAlert(ent, alertProto);
    }
}
