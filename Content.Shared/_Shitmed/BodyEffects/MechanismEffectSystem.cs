using Content.Shared._Shitmed.Body.Components;
using Content.Shared._Shitmed.Body.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;
using System.Linq;
using Robust.Shared.Network;

namespace Content.Shared._Shitmed.BodyEffects;

public partial class MechanismEffectSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly ISerializationManager _serManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly INetManager _net = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechanismEffectComponent, MechanismEnabledEvent>(OnEnabled);
        SubscribeLocalEvent<MechanismEffectComponent, MechanismDisabledEvent>(OnDisabled);
    }

    // While I would love to kill this function, problem is that if we happen to have two parts that add the same
    // effect, removing one will remove both of them, since we cant tell what the source of a Component is.
    // TODO: use refcounting in 5 months
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_net.IsServer) // TODO: Kill this once I figure out whats breaking the Diagnostic Cybernetics.
            return;

        var query = EntityQueryEnumerator<MechanismEffectComponent, BodyMechanismComponent>();
        var now = _gameTiming.CurTime;
        while (query.MoveNext(out var uid, out var comp, out var part))
        {
            if (now < comp.NextUpdate || !comp.Active.Any() || part.Body is not { } body)
                continue;

            comp.NextUpdate = now + comp.Delay;
            AddComponents(body, (uid, comp), comp.Active);
        }
    }

    private void OnEnabled(Entity<MechanismEffectComponent> ent, ref MechanismEnabledEvent args)
    {
        if (ent.Comp.Added is {} add)
            AddComponents(args.Body, ent, add);
        if (ent.Comp.Removed is {} remove)
            RemoveComponents(args.Body, ent, remove);
    }

    private void OnDisabled(Entity<MechanismEffectComponent> ent, ref MechanismDisabledEvent args)
    {
        if (ent.Comp.Added is {} add)
            RemoveComponents(args.Body, ent, add);
        if (ent.Comp.Removed is {} remove)
            AddComponents(args.Body, ent, remove);
    }

    private void AddComponents(EntityUid body,
        Entity<MechanismEffectComponent> part,
        ComponentRegistry reg)
    {
        foreach (var (key, comp) in reg)
        {
            var compType = comp.Component.GetType();
            if (HasComp(body, comp.Component.GetType()))
                continue;

            part.Comp.Active[key] = comp;
        }
        Dirty(part);

        EntityManager.AddComponents(body, reg);
    }

    private void RemoveComponents(EntityUid body,
        Entity<MechanismEffectComponent> part,
        ComponentRegistry reg)
    {
        foreach (var key in reg.Keys)
        {
            part.Comp.Active.Remove(key);
        }
        Dirty(part);

        EntityManager.RemoveComponents(body, reg);
    }
}
