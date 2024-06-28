using Content.Server.SimpleStation14.Silicon.Death;
using Content.Server.Sound.Components;
using Content.Shared.Mobs;
using Content.Shared.SimpleStation14.Silicon.Systems;

namespace Content.Server.SimpleStation14.Silicon;

public sealed class EmitSoundOnCritSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<SiliconEmitSoundOnDrainedComponent, SiliconChargeDeathEvent>(OnDeath);
        SubscribeLocalEvent<SiliconEmitSoundOnDrainedComponent, SiliconChargeAliveEvent>(OnAlive);
        SubscribeLocalEvent<SiliconEmitSoundOnDrainedComponent, MobStateChangedEvent>(OnStateChange);
    }

    private void OnDeath(EntityUid uid, SiliconEmitSoundOnDrainedComponent component, SiliconChargeDeathEvent args)
    {
        var spamComp = EnsureComp<SpamEmitSoundComponent>(uid);

        spamComp.Accumulator = 0f;
        spamComp.RollInterval = component.Interval;
        spamComp.PlayChance = component.PlayChance;
        spamComp.PopUp = component.PopUp;
        spamComp.Enabled = true;
        spamComp.Sound = component.Sound;
    }

    private void OnAlive(EntityUid uid, SiliconEmitSoundOnDrainedComponent component, SiliconChargeAliveEvent args)
    {
        RemComp<SpamEmitSoundComponent>(uid); // This component is bad and I don't feel like making a janky work around because of it.
        // If you give something the SiliconEmitSoundOnDrainedComponent, know that it can't have the SpamEmitSoundComponent, and any other systems that play with it will just be broken.
    }

    public void OnStateChange(EntityUid uid, SiliconEmitSoundOnDrainedComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            RemComp<SpamEmitSoundComponent>(uid);
    }
}
