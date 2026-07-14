using Content.Shared._DV.Clothing.Events;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.StatusEffectNew.Components;

namespace Content.Shared.StatusEffectNew;

/// <summary>
/// Partial file for DeltaV specific Status Effects to avoid upstream merge conflicts.
/// </summary>
public sealed partial class StatusEffectsSystem
{
    private void InitializeDeltaV()
    {
        // Psionics
        SubscribeLocalEvent<StatusEffectContainerComponent, PsionicPowerUseAttemptEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, TargetedByPsionicPowerEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, DispelledEvent>(RelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, PsionicSuppressedEvent>(RefRelayStatusEffectEvent);

        // Generic
        SubscribeLocalEvent<StatusEffectContainerComponent, ModifySlowOnDamageSpeedEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, ModifyClothingSlowdownEvent>(RefRelayStatusEffectEvent);
    }
}
