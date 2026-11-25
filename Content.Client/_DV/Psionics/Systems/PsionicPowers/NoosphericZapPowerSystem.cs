using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;

namespace Content.Client._DV.Psionics.Systems.PsionicPowers;

/// <summary>
/// This is solely for prediction.
/// </summary>
public sealed class NoosphericZapPowerSystem : SharedNoosphericZapPowerSystem
{
    protected override void OnPowerUsed(Entity<NoosphericZapPowerComponent> psionic, ref NoosphericZapPowerActionEvent args)
    {
    }
}
