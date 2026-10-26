using Content.Shared._DV.Medical.Healing;
using Content.Shared.Tools.Systems;

namespace Content.Shared.Medical.Healing;

public sealed partial class HealingSystem
{
    [Dependency] private readonly SharedToolSystem _tool = default!;

    /// <summary>
    /// Checks for ToolHealingComponent, and if it exists, check if the tool can be used
    /// </summary>
    private bool CanUseTool(EntityUid healing, EntityUid target, EntityUid user)
    {
        // let healing go ahead if there's no tool healing
        if (!TryComp<ToolHealingComponent>(healing, out var toolHealing))
            return true;

        return _tool.CanStartToolUse(healing, user, target, toolHealing.Fuel, []);
    }
}
