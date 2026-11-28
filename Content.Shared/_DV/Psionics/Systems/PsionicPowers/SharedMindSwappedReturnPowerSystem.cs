using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

/// <summary>
/// This is solely for prediction. Everything is in the server-side file.
/// </summary>
public abstract class SharedMindSwappedReturnPowerSystem : BasePsionicPowerSystem<MindSwappedReturnPowerComponent, MindSwappedReturnPowerActionEvent>;
