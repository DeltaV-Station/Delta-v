using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;

namespace Content.Client._DV.Psionics.Systems.PsionicPowers;

/// <summary>
/// This solely exists for prediction.
/// </summary>
public sealed class MassSleepPowerSystem : SharedMassSleepPowerSystem
{
    protected override void OnPowerUsed(Entity<MassSleepPowerComponent> psionic, ref MassSleepPowerActionEvent args)
    {
    }
}
