using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared._DV.Abilities.Kitsune;
using Content.Shared.Actions;
using Content.Shared.Damage.Systems;
using Content.Shared.Polymorph;
using Content.Shared.Stunnable;

namespace Content.Server._DV.Abilities.Kitsune;

public sealed class KitsuneFoxSystem : EntitySystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KitsuneFoxComponent, StunnedEvent>(OnStunned);
        SubscribeLocalEvent<KitsuneFoxComponent, PolymorphedEvent>(OnPolymorphed);

    }

    private void OnPolymorphed(Entity<KitsuneFoxComponent> ent, ref PolymorphedEvent args)
    {
        if(!TryComp<PolymorphedEntityComponent>(ent, out var polymorph))
            return;

        if(!TryComp<KitsuneComponent>(polymorph.Parent, out var kitsune))
            return;

        if(kitsune.FoxfireAction is not {} foxfireAction)
            return;

        _actions.AddActionDirect(ent, foxfireAction);
    }

    private void OnStunned(Entity<KitsuneFoxComponent> ent, ref StunnedEvent args)
    {
        if (!TryComp<PolymorphedEntityComponent>(ent, out var polymorph))
            return;
        var staminaDamage = _stamina.GetStaminaDamage(ent);
        _stamina.TakeStaminaDamage(polymorph.Parent, staminaDamage);
        _polymorph.Revert(ent.Owner);
    }
}
