using Content.Server.Bible.Components;
using Content.Server.Guardian;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;

namespace Content.Server._DV.Psionics.Systems.PsionicPowers;

public sealed class DispelPowerSystem : SharedDispelPowerSystem
{
    [Dependency] private readonly GuardianSystem _guardian = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GuardianComponent, DispelledEvent>(OnGuardianDispelled);
        SubscribeLocalEvent<FamiliarComponent, DispelledEvent>(OnFamiliarDispelled);
    }

    private void OnGuardianDispelled(Entity<GuardianComponent> guardian, ref DispelledEvent args)
    {
        if (TryComp<GuardianHostComponent>(guardian.Comp.Host, out var host))
            _guardian.ToggleGuardian(guardian.Comp.Host.Value, host);

        DealDispelDamage(guardian, dispeller: args.Dispeller);
        args.Handled = true;
    }

    private void OnFamiliarDispelled(Entity<FamiliarComponent> familiar, ref DispelledEvent args)
    {
        if (familiar.Comp.Source != null)
            EnsureComp<SummonableRespawningComponent>(familiar.Comp.Source.Value);

        args.Handled = true;
    }
}
