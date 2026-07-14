using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;

namespace Content.Client._DV.Psionics.Systems.PsionicPowers;

/// <summary>
/// This exists solely for prediction.
/// </summary>
public sealed class PyrokinesisPowerSystem : SharedPyrokinesisPowerSystem
{
    protected override void OnPowerUsed(Entity<PyrokinesisPowerComponent> psionic, ref Shared._DV.Psionics.Events.PowerActionEvents.PyrokinesisPowerActionEvent args)
    {
    }
}
