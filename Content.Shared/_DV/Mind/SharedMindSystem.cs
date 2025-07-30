using Content.Shared.Mind.Components;

namespace Content.Shared.Mind;

public partial class SharedMindSystem
{
    /// <summary>
    /// Set whether or not to show examine information about the mind. Used to obscure if a mind is SSD or not.
    /// </summary>
    /// <param name="uid">Entity to set the mind examine info for.</param>
    /// <param name="showExamineInfo">True to show examine information, false to hide it.</param>
    public void ShowExamineInfo(EntityUid uid, bool showExamineInfo)
    {
        if (!TryComp<MindContainerComponent>(uid, out var mindContainer))
            return;

        mindContainer.ShowExamineInfo = showExamineInfo;
        Dirty(uid, mindContainer);
    }
}
