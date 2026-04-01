using Content.Client._DV.Traits.UI;
using Content.Shared._DV.CCVars;
using Content.Shared._DV.Traits;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.Traits;

/// <summary>
/// Client system that shows a popup when traits are disabled due to unmet conditions.
/// </summary>
public sealed class DisabledTraitsPopupSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private DisabledTraitsPopup? _window;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<DisabledTraitsEvent>(OnDisabledTraits);
    }

    private void OnDisabledTraits(DisabledTraitsEvent ev)
    {
        // Don't show if user has opted to skip this popup
        if (_cfg.GetCVar(DCCVars.SkipDisabledTraitsPopup))
            return;

        // Don't show if no traits were actually disabled
        if (ev.DisabledTraits.Count == 0)
            return;

        OpenDisabledTraitsPopup(ev.DisabledTraits);
    }

    private void OpenDisabledTraitsPopup(Dictionary<ProtoId<TraitPrototype>, List<string>> disabledTraits)
    {
        // Close existing window if one is open
        if (_window != null)
        {
            _window.Close();
            _window = null;
        }

        _window = new DisabledTraitsPopup(disabledTraits);
        _window.OpenCentered();
        _window.OnClose += () => _window = null;
    }
}
