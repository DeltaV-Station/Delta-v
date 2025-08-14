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

    private static ProtoId<AlertPrototype> _lightLevelDarkIcon = "LightLevelDarkIcon";
    private static ProtoId<AlertPrototype> _lightLevelNeutralIcon = "LightLevelNeutralIcon";
    private static ProtoId<AlertPrototype> _lightLevelBrightIcon = "LightLevelBrightIcon";
    private static ProtoId<AlertCategoryPrototype> _lightAlertCategory = "Light";

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
            if(_prototype.Index(_lightAlertCategory) is { } alertCategory)
                _alerts.ClearAlertCategory(ent, alertCategory);
        }
        _lastThreshold = currentThreshold;

        if (_prototype.Index(alertIcon) is not { } alertProto)
            return;

        _alerts.ShowAlert(ent, alertProto);
    }
}
