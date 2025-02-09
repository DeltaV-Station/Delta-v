using Content.Shared.Actions.Events;
using Content.Shared.Abilities.Psionics;

namespace Content.Shared.Abilities.Kitsune;

public sealed class KitsuneSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        {
            base.Initialize();

            SubscribeLocalEvent<PsionicComponent, CreateFoxfireActionEvent>(OnCreateFoxfire);
        }
    }

    private void OnCreateFoxfire(EntityUid uid, PsionicComponent component, CreatefoxfireActionEvent args)
    {
        if (HasComp<PsionicInsulationComponent>(uid))
            return;

        var fireEnt = Spawn(args.FoxfirePrototype, Transform(uid).Coordinates);
        var fireComp = EnsureComp<FoxFireComponent>(fireEnt);
        fireComp.Owner = uid;

        args.Handled = true;
    }
}
