using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared._DV.Abilities.Kitsune;
using Content.Shared.Damage.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;

namespace Content.Server._DV.Abilities.Kitsune;

public sealed class KitsuneFoxSystem : EntitySystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KitsuneFoxComponent, StunnedEvent>(OnStunned);
    }

    private void OnStunned(EntityUid uid, KitsuneFoxComponent component, StunnedEvent e)
    {
        var staminaDamage = _stamina.GetStaminaDamage(uid);
        _polymorph.Revert(uid);
        if (!TryComp<PolymorphedEntityComponent>(uid, out var polymorphedEntity))
            return;
        _stamina.TakeStaminaDamage(polymorphedEntity.Parent, staminaDamage);
    }
}
