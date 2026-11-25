using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;

namespace Content.Client._DV.Psionics.Systems.PsionicPowers;

/// <summary>
/// This is here solely for predictive handling of using the power and not being able to.
/// It'll send popups this way.
/// </summary>
public sealed class TelegnosisPowerSystem : SharedTelegnosisPowerSystem
{
    protected override void OnPowerUsed(Entity<TelegnosisPowerComponent> psionic, ref TelegnosisPowerActionEvent args)
    {
    }
}
