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
        if (!TryComp<KitsuneComponent>(args.NewEntity, out var newKitsune)
            || !TryComp<KitsuneComponent>(ent, out var oldKitsune))
            return;
        newKitsune.ActiveFoxFires = oldKitsune.ActiveFoxFires;

        _actions.SetCharges(newKitsune.FoxfireAction, _actions.GetCharges(oldKitsune.FoxfireAction));

        foreach (var foxFire in newKitsune.ActiveFoxFires)
        {
            if(!TryComp<FoxfireComponent>(foxFire, out var foxfire))
                continue;
            foxfire.Kitsune = args.NewEntity;
        }
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
