using Content.Shared.Body;
using Content.Shared.Body.Events;
using Content.Shared.Chemistry.Reagent; // DeltaV
using Robust.Shared.Prototypes; // DeltaV

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

    /// <summary>
    /// DeltaV - check if target can metabolize a given reagent
    /// </summary>
    public bool BodyCanMetabolizeReagent(EntityUid ent, ProtoId<ReagentPrototype> reagent, BodyComponent? comp = null)
    {
        if (!Resolve(ent, ref comp, false))
            return true;

        var ev = new MetabolismWhitelistEvent();
        RaiseLocalEvent(ent, ref ev);

        // this is wrong if the whitelist is empty,
        // but it's annoying to determine if MetabolizerWhitelistComponent exists
        return ev.Reagents.Count == 0 || ev.Reagents.Contains(reagent);
    }
}
