using Content.Shared.Research.Components;
using Content.Shared.Xenoarchaeology.BUI;
using Content.Shared.Xenoarchaeology.Equipment.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Xenoarchaeology.Ui;

/// <summary>
/// BUI for artifact analysis console, proxies server-provided UI updates
/// (related to device, connected artifact analyzer, and artifact lying on it).
/// </summary>
[UsedImplicitly]
public sealed class AnalysisConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private AnalysisConsoleMenu? _consoleMenu;

    /// <inheritdoc />
    protected override void Open()
    {
        base.Open();

        _consoleMenu = this.CreateWindow<AnalysisConsoleMenu>();
        _consoleMenu.SetOwner(owner);

        _consoleMenu.OnClose += Close;
        _consoleMenu.OpenCentered();

        _consoleMenu.OnServerSelectionButtonPressed += () =>
        {
            SendMessage(new ConsoleServerSelectionMessage());
        };
        _consoleMenu.OnExtractButtonPressed += () =>
        {
            SendMessage(new AnalysisConsoleExtractButtonPressedMessage());
        };
    }

    /// <summary>
    /// Update UI state based on corresponding component.
    /// </summary>
    public void Update(Entity<AnalysisConsoleComponent> ent)
    {
        _consoleMenu?.Update(ent);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _consoleMenu?.Dispose();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not AnalysisConsoleBoundUserInterfaceState consoleState)
            return;

        _consoleMenu?.UpdateState(consoleState.Mult, consoleState.Ratio);
    }

}

