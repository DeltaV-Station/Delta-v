using System.IO;
using Content.Shared._DV.Silicons;
using Content.Shared.Fax;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Containers;

namespace Content.Client._DV.Silicons;

[UsedImplicitly]
public sealed class StationAiFaxBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private readonly IFileDialogManager _fileDialogManager = default!;

    private StationAiFaxWindow? _window;
    private List<StationAiFaxPageWindow> _windows = new();

    private bool _dialogIsOpen;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<StationAiFaxWindow>();
        _window.FileButtonPressed += OnFileButtonPressed;
        _window.BlankButtonPressed += OnBlankButtonPressed;
        _window.OpenSendWindowPressed += OnOpenSendWindowPressed;
        _window.ExaminePressed += OnExaminePressed;
        _window.ShredPressed += OnShredPressed;
        _window.DuplicatePressed += OnDuplicatePressed;

        UpdateFaxes(EntMan.System<SharedContainerSystem>().EnsureContainer<Container>(Owner, StationAiFaxComponent.Container, out _));
    }

    private async void OnFileButtonPressed()
    {
        if (_dialogIsOpen)
            return;

        _dialogIsOpen = true;
        var filters = new FileDialogFilters(new FileDialogFilters.Group("txt"));
        await using var file = await _fileDialogManager.OpenFile(filters, FileAccess.Read);
        _dialogIsOpen = false;

        if (_window == null || _window.Disposed || file == null)
        {
            return;
        }

        using var reader = new StreamReader(file);

        var firstLine = await reader.ReadLineAsync();
        string? label = null;
        var content = await reader.ReadToEndAsync();

        if (firstLine is { })
        {
            if (firstLine.StartsWith('#'))
            {
                label = firstLine[1..].Trim();
            }
            else
            {
                content = firstLine + "\n" + content;
            }
        }

        SendMessage(new FaxFileMessage(
            label?[..Math.Min(label.Length, FaxFileMessageValidation.MaxLabelSize)],
            content[..Math.Min(content.Length, FaxFileMessageValidation.MaxContentSize)],
            false));
    }

    private void OnBlankButtonPressed()
    {
        SendMessage(new FaxFileMessage(null, string.Empty, false));
    }

    private void OnOpenSendWindowPressed(EntityUid entity)
    {
        var window = new StationAiFaxPageWindow(entity, EntMan);
        _windows.Add(window);

        window.RefreshButtonPressed += OnRefreshButtonPressed;
        window.PeerSelected += OnPeerSelected;
        window.SendButtonPressed += OnSendDocument;

        if (UiSystem.TryGetUiState<FaxUiState>(Owner, UiKey, out var state))
        {
            window.UpdateState(state);
        }

        window.OpenCentered();
    }

    private void OnSendDocument(EntityUid entity)
    {
        SendMessage(new StationAiFaxSendMessage(EntMan.GetNetEntity(entity)));
    }

    private void OnExaminePressed(EntityUid entity)
    {
        SendPredictedMessage(new StationAiFaxExamineMessage(EntMan.GetNetEntity(entity)));
    }

    private void OnShredPressed(EntityUid entity)
    {
        SendPredictedMessage(new StationAiFaxShredMessage(EntMan.GetNetEntity(entity)));
    }

    private void OnDuplicatePressed(EntityUid entity)
    {
        SendPredictedMessage(new StationAiFaxDuplicateMessage(EntMan.GetNetEntity(entity)));
    }

    private void OnRefreshButtonPressed()
    {
        SendMessage(new FaxRefreshMessage());
    }

    private void OnPeerSelected(string address)
    {
        SendMessage(new FaxDestinationMessage(address));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not FaxUiState cast)
            return;

        _window?.UpdateState(cast);
        foreach (var window in _windows)
        {
            window.UpdateState(cast);
        }
    }

    public void UpdateFaxes(Container container)
    {
        _window?.UpdateFaxes(container);
    }
}
