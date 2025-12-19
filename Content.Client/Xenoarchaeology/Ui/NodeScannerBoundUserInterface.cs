using Content.Client._DV.Xenoarchaeology.Ui;
using Robust.Client.UserInterface;

namespace Content.Client.Xenoarchaeology.Ui;

/// <summary>
/// BUI for hand-held xeno artifact scanner,  server-provided UI updates.
/// </summary>
public sealed class NodeScannerBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    // DeltaV - start of node scanner overhaul
    [ViewVariables]
    private DVNodeScannerDisplay? _scannerDisplay;
    // DeltaV - end of node scanner overhaul

    /// <inheritdoc />
    protected override void Open()
    {
        base.Open();

        // DeltaV - start of node scanner overhaul
        _scannerDisplay = this.CreateWindow<DVNodeScannerDisplay>();
        // DeltaV - end of node scanner overhaul
        _scannerDisplay.SetOwner(Owner);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _scannerDisplay?.Dispose();
    }
}
