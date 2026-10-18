using Content.Shared._DV.Medical.Healing;
using Content.Shared.Tools.Components;

namespace Content.Shared.Medical.Healing;

public sealed partial class HealingSystem
{

    /// <summary>
    /// Checks for ToolHealingComponent, and if it exists, check if the tool can be used
    /// </summary>
    private bool CanUseTool(EntityUid healing, EntityUid target, EntityUid user)
    {
        // let healing go ahead if there's no tool healing
        if (!TryComp<ToolHealingComponent>(healing, out var toolHealing))
            return true;

        // check if the user allows using the tool
        var ev = new ToolUserAttemptUseEvent(target);
        RaiseLocalEvent(user, ref ev);
        if (ev.Cancelled)
            return false;

        // check if the tool allows being used
        var beforeAttempt = new ToolUseAttemptEvent(user, toolHealing.Fuel);
        RaiseLocalEvent(healing, beforeAttempt);
        if (beforeAttempt.Cancelled)
            return false;

        return true;
    }
}
