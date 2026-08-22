using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;

namespace Content.Client._DV.Psionics.Systems.PsionicPowers;

/// <summary>
/// This is solely for prediction. Everything is in the server-side file.
/// </summary>
public sealed class MindSwappedReturnPowerSystem : SharedMindSwappedReturnPowerSystem
{
    protected override void OnPowerUsed(Entity<MindSwappedReturnPowerComponent> psionic, ref MindSwappedReturnPowerActionEvent args)
    {
    }
}
