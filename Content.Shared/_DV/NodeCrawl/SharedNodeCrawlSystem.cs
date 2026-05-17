using Content.Shared.Eye;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.NodeCrawl;

/// <summary>
/// Manages entry & exit of node crawlers into node networks
/// </summary>
public abstract class SharedNodeCrawlSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;

    private const string MoverContainer = "mover-container";
    private static readonly EntProtoId MoverProto = "DVNodeCrawlMover";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NodeCrawlerComponent, GetVerbsEvent<InnateVerb>>(OnGetVerbs);
        SubscribeLocalEvent<NodeCrawlerComponent, NodeCrawlerArrivedAtNodeEvent>(OnArrivedAtNode);
        SubscribeLocalEvent<NodeCrawlerComponent, GetVisMaskEvent>(OnGetVisMask);
    }

    private void OnGetVerbs(Entity<NodeCrawlerComponent> ent, ref GetVerbsEvent<InnateVerb> args)
    {
        var target = args.Target;
        if (!HasComp<NodeCrawlComponent>(target))
            return;

        if (!_entityWhitelist.IsWhitelistPass(ent.Comp.ExitNodes, target))
            return;

        args.Verbs.Add(new InnateVerb
        {
            Act = () =>
            {
                NodeCrawl(ent, target);
            },
            Text = Loc.GetString("node-crawl-enter", ("target", target)),
        });
    }

    private void NodeCrawl(Entity<NodeCrawlerComponent> ent, EntityUid target)
    {
        if (!_net.IsServer)
            return;

        var mover = Spawn(MoverProto, Transform(target).Coordinates);
        var crawler = Comp<NodeCrawlerMovementComponent>(mover);

        var container = _container.GetContainer(mover, MoverContainer);
        _container.Insert(ent.Owner, container);

        ent.Comp.Mover = mover;
        Dirty(ent);

        crawler.Node = target;
        Dirty(mover, crawler);

        _mover.SetRelay(ent, mover);
        _physics.SetCanCollide(ent.Owner, false);
        _physics.SetCanCollide(mover, false);
        _eye.RefreshVisibilityMask(ent.Owner);
    }

    private void ExitNodeCrawl(Entity<NodeCrawlerComponent> ent)
    {
        if (ent.Comp.Mover is not { } mover)
            return;

        ent.Comp.Mover = null;
        Dirty(ent);

        var container = _container.GetContainer(mover, MoverContainer);
        _container.Remove(ent.Owner, container);
        RemComp<RelayInputMoverComponent>(ent);
        Del(mover);

        _physics.SetCanCollide(ent.Owner, true);
        _eye.RefreshVisibilityMask(ent.Owner);
    }

    private void OnArrivedAtNode(Entity<NodeCrawlerComponent> ent, ref NodeCrawlerArrivedAtNodeEvent args)
    {
        if (!_entityWhitelist.IsWhitelistPass(ent.Comp.ExitNodes, args.Node))
            return;

        ExitNodeCrawl(ent);
    }

    private void OnGetVisMask(Entity<NodeCrawlerComponent> ent, ref GetVisMaskEvent args)
    {
        if (ent.Comp.Mover is null)
            return;

        args.VisibilityMask |= (int)VisibilityFlags.Subfloor;
    }
}
