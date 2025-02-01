using Content.Shared.Hands.Components;
using Robust.Shared.Player;

namespace Content.Shared.Hands.EntitySystems;

public partial class SharedHandsSystem
{
    /// <summary>
    /// Swap hands in the right direction
    /// </summary>
    /// <param name="session">Session data</param>
    private void SwapHandsPressed(ICommonSession? session)
    {
        ChangeHandIndex(session, 1);
    }

    /// <summary>
    /// Will swap hands in the left direction
    /// </summary>
    /// <param name="session">Session data</param>
    private void SwapHandsReversedPressed(ICommonSession? session)
    {
        ChangeHandIndex(session, -1);
    }

    /// <summary>
    /// Will swap hands relative to the current hand position using the given modifier.
    /// </summary>
    /// <param name="session">Session data</param>
    /// <param name="modifier">positive or negative value</param>
    private void ChangeHandIndex(ICommonSession? session, int modifier)
    {
        if (!TryComp(session?.AttachedEntity, out HandsComponent? component))
            return;
        if (!_actionBlocker.CanInteract(session.AttachedEntity.Value, null))
            return;
        if (component.ActiveHand == null || component.Hands.Count < 2)
            return;

        var newActiveIndex = component.SortedHands.IndexOf(component.ActiveHand.Name) + modifier;
        while (newActiveIndex < 0)
        {
            newActiveIndex += component.SortedHands.Count;
        }

        var nextHand = component.SortedHands[newActiveIndex % component.Hands.Count];

        TrySetActiveHand(session.AttachedEntity.Value, nextHand, component);
    }
}
