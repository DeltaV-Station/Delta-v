// SPDX-FileCopyrightText: 2026 taydeo <tay@funkystation.org>
//
// SPDX-License-Identifier: MIT

using Content.Client.Gameplay;
using Content.Client.Lobby;
using Content.Shared._Funkystation.CCVars;
using JetBrains.Annotations;
using Robust.Client.Console;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Configuration;

namespace Content.Client._Funkystation.ContentWarning;

[UsedImplicitly]
public sealed class ContentWarningUIController : UIController, IOnStateEntered<LobbyState>, IOnStateEntered<GameplayState>
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IClientConsoleHost _consoleHost = default!;

    private ContentWarningPopup? _window;

    private void AttemptOpenContentWarningPopup()
    {
        if (!_cfg.GetCVar(CCVars_Funky.ContentWarningDisplay) || _cfg.GetCVar(CCVars_Funky.ContentWarningAcknowledged))
            return;

        OpenContentWarningPopup();
    }

    public void OnStateEntered(LobbyState _)
    {
        AttemptOpenContentWarningPopup();
    }

    public void OnStateEntered(GameplayState _)
    {
        AttemptOpenContentWarningPopup();
    }

    private void OpenContentWarningPopup()
    {
        if (_window != null)
            return;

        _window = new ContentWarningPopup();
        _window.OpenCentered();
        _window.OnContentWarningReject += () =>
        {
            _window.Close();
            _window = null;

            if (_cfg.GetCVar(CCVars_Funky.ContentWarningKickOnIgnore))
                _consoleHost.ExecuteCommand("quit");
        };
        _window.OnContentWarningAccept += () =>
        {
            _window.Close();
            _window = null;
            _cfg.SetCVar(CCVars_Funky.ContentWarningAcknowledged, true);
            _cfg.SaveToFile();
        };
    }
}
