using Content.Shared.Body;
using Content.Shared.Body.Events;

namespace Content.Shared._starcup.Metabolism;

public sealed class MetabolizerWhitelistSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MetabolizerWhitelistComponent, BodyRelayedEvent<MetabolismWhitelistEvent>>(OnMetabolismWhitelistCheck);
    }

    private static void OnMetabolismWhitelistCheck(Entity<MetabolizerWhitelistComponent> ent, ref BodyRelayedEvent<MetabolismWhitelistEvent> args)
    {
        if (ent.Comp.ReagentWhitelist == null)
            return;

        foreach (var reagent in ent.Comp.ReagentWhitelist)
        {
            args.Args.Reagents.Add(reagent);
        }
    }
}
