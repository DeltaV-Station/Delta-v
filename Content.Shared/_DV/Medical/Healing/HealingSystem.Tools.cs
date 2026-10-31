using Content.Shared._DV.Medical.Healing;
using Content.Shared.Tools.Systems;

// ReSharper disable once CheckNamespace
namespace Content.Shared.Medical.Healing;

public sealed partial class HealingSystem
{
    [Dependency] private readonly SharedToolSystem _tool = default!;

    // UseTool forbids literals
    private readonly List<string> EmptyToolQualities = [];

    /// <summary>
    /// Tries to use the healing item as a tool, using a tool doafter.
    /// If the healing item does not have the ToolHealingComponent, the doafter will not be initiated.
    /// </summary>
    /// <remarks>
    /// Returns true if the item should be used as a tool, false if not.
    /// toolUseSuccess is set to true if tool use was successful, false if not.
    /// </remarks>
    private bool TryUseAsTool(
        Entity<HealingComponent> healing,
        EntityUid target,
        EntityUid user,
        TimeSpan doAfterDelay,
        out bool toolUseSuccess)
    {
        toolUseSuccess = false;

        if (!TryComp<ToolHealingComponent>(healing, out var toolHealing))
            return false;

        toolUseSuccess = _tool.UseTool(healing, user, target, doAfterDelay,
            EmptyToolQualities, new HealingDoAfterEvent(), out _, toolHealing.Fuel, breakOnDamage:false);
        return true;
    }
}
