using Content.Client._DV.Tips.UI;
using Content.Shared._DV.CCVars;
using Content.Shared._DV.Tips;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.Tips;

/// <summary>
/// Client-side system that receives tip events from the server and displays the tip popup UI.
/// </summary>
public sealed class TipSystem : SharedTipSystem
{
    [Dependency] private readonly IUserInterfaceManager _ui = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    /// <summary>
    /// Queue of tips waiting to be shown. Only one tip is displayed at a time.
    /// </summary>
    private readonly Queue<ShowTipEvent> _tipQueue = new();

    /// <summary>
    /// Currently displayed tip popup, if any.
    /// </summary>
    private TipPopup? _currentPopup;

    /// <summary>
    /// The tip ID of the currently displayed popup, used for dismiss events.
    /// </summary>
    private ProtoId<TipPrototype>? _currentTipId;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<ShowTipEvent>(OnShowTip);
    }

    private void OnShowTip(ShowTipEvent ev)
    {
        // Return if tips are disabled globally
        if (_cfg.GetCVar(DCCVars.DisableTipsGlobal))
            return;

        // Return if tips are disabled locally and IgnoreCvar is the default value
        if (_cfg.GetCVar(DCCVars.DisableTips) && !ev.IgnoreCvar)
            return;

        _tipQueue.Enqueue(ev);
        TryShowNextTip();
    }

    private void TryShowNextTip()
    {
        while (true)
        {
            if (_currentPopup != null)
                return;

            if (_tipQueue.Count == 0)
                return;

            var tipEvent = _tipQueue.Dequeue();

            if (!Prototype.TryIndex(tipEvent.TipId, out var tipProto))
            {
                Log.Warning($"Received tip event for unknown prototype: {tipEvent.TipId}");
                continue;
            }

            ShowTipPopup(tipProto, tipEvent);
            break;
        }
    }

    private void ShowTipPopup(TipPrototype tip, ShowTipEvent ev)
    {
        _currentPopup = new TipPopup(tip);
        _currentPopup.OnClose += OnPopupClosed;
        _currentTipId = tip.ID;

        _ui.RootControl.AddChild(_currentPopup);

        // Play the sound
        _audio.PlayGlobal(ev.Sound, Filter.Local(), false);
    }

    private void OnPopupClosed(bool dontShowAgain)
    {
        if (_currentPopup != null && _currentTipId != null)
        {
            // Send dismiss event to server
            RaiseNetworkEvent(new TipDismissedEvent(_currentTipId.Value, dontShowAgain));

            _currentPopup.OnClose -= OnPopupClosed;
            _currentPopup?.Close();
            _currentPopup = null;
            _currentTipId = null;
        }

        TryShowNextTip();
    }

    /// <summary>
    /// Closes the current tip popup if one is open.
    /// </summary>
    [PublicAPI]
    public void CloseCurrentTip()
    {
        _currentPopup?.Close();
    }

    /// <summary>
    /// Clears all queued tips without showing them.
    /// </summary>
    [PublicAPI]
    public void ClearTipQueue()
    {
        _tipQueue.Clear();
    }
}
