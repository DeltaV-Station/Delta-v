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

    private ProtoId<AlertPrototype> _lightLevelDarkIcon = "LightLevelDarkIcon";
    private ProtoId<AlertPrototype> _lightLevelNeutralIcon = "LightLevelNeutralIcon";
    private ProtoId<AlertPrototype> _lightLevelBrightIcon = "LightLevelBrightIcon";
    private ProtoId<AlertCategoryPrototype> _lightAlertCategory = "Light";

    private int _lastThreshold = 0;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // If we're a LightLevelHealthComponent client, update our alerts.
        if (_player.LocalSession?.AttachedEntity is not { } ent)
            return;

        if (!TryComp<LightLevelHealthComponent>(ent, out var lightLevelHealth))
            return;

        var currentThreshold = CurrentThreshold(_lightReactive.GetLightLevelForPoint(ent), lightLevelHealth);
        var alertIcon = currentThreshold switch
        {
            -1 => _lightLevelDarkIcon,
            1 => _lightLevelBrightIcon,
            _ => _lightLevelNeutralIcon,
        };

        if (currentThreshold != _lastThreshold)
        {
            var alertCategory = _prototype.Index(_lightAlertCategory);
            _alerts.ClearAlertCategory(ent, alertCategory);
        }
        _lastThreshold = currentThreshold;

        var alertProto = _prototype.Index(alertIcon);
        _alerts.ShowAlert(ent, alertProto);
    }
}
