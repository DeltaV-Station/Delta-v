using Content.Shared.Eye;
using Content.Shared.Interaction;
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
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly NodeCrawlerMovementSystem _nodeCrawler = default!;

    private const string MoverContainer = "mover-container";
    private static readonly EntProtoId MoverProto = "DVNodeCrawlMover";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NodeCrawlerComponent, GetVerbsEvent<InnateVerb>>(OnGetVerbs);
        SubscribeLocalEvent<NodeCrawlerComponent, NodeCrawlerArrivedAtNodeEvent>(OnArrivedAtNode);
        SubscribeLocalEvent<NodeCrawlerComponent, GetVisMaskEvent>(OnGetVisMask);

        SubscribeLocalEvent<CrawlableNodeComponent, ComponentShutdown>(OnCrawlableShutdown);
        SubscribeLocalEvent<NodeCrawlerMovementComponent, ComponentShutdown>(OnMovementShutdown);
        SubscribeLocalEvent<NodeCrawlerComponent, ComponentShutdown>(OnCrawlerShutdown);

        SubscribeLocalEvent<CrawlableNodeComponent, AnchorStateChangedEvent>(OnCrawlableAnchorChanged);
    }

    private void OnGetVerbs(Entity<NodeCrawlerComponent> ent, ref GetVerbsEvent<InnateVerb> args)
    {
        var target = args.Target;
        if (!HasComp<CrawlableNodeComponent>(target))
            return;

        if (!_entityWhitelist.IsWhitelistPass(ent.Comp.ExitNodes, target))
            return;

        if (!_interaction.InRangeAndAccessible(ent.Owner, target))
            return;

        args.Verbs.Add(new InnateVerb
        {
            Act = () =>
            {
                if (!_interaction.InRangeAndAccessible(ent.Owner, target))
                    return;

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

        _nodeCrawler.SetNode((mover, crawler), target);
        _nodeCrawler.SetHeldCrawler((mover, crawler), ent);

        _mover.SetRelay(ent, mover);
        _physics.SetCanCollide(ent.Owner, false);
        _physics.SetCanCollide(mover, false);
        _eye.RefreshVisibilityMask(ent.Owner);
    }

    /// <summary>
    /// Causes this node crawler to exit its node crawl.
    /// </summary>
    /// <param name="ent">The crawler to exit node-crawl from.</param>
    public void ExitNodeCrawl(Entity<NodeCrawlerComponent> ent)
    {
        if (ent.Comp.Mover is not { } mover)
            return;

        ent.Comp.Mover = null;
        Dirty(ent);

        var container = _container.GetContainer(mover, MoverContainer);
        _container.Remove(ent.Owner, container);
        RemComp<RelayInputMoverComponent>(ent);
        if (_net.IsServer && !TerminatingOrDeleted(mover))
            QueueDel(mover);

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

    private void OnCrawlableShutdown(Entity<CrawlableNodeComponent> ent, ref ComponentShutdown args)
    {
        foreach (var crawler in ent.Comp.Crawlers)
        {
            var movement = Comp<NodeCrawlerMovementComponent>(crawler);
            if (movement.HeldCrawler is not { } held)
                continue;

            _nodeCrawler.SetNode((crawler, movement), null);
            ExitNodeCrawl((held, Comp<NodeCrawlerComponent>(held)));
        }
    }

    private void OnMovementShutdown(Entity<NodeCrawlerMovementComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Node is { } node)
        {
            var nodeComp = Comp<CrawlableNodeComponent>(node);
            nodeComp.Crawlers.Remove(ent);
            Dirty(node, nodeComp);
        }

        if (ent.Comp.HeldCrawler is { } crawler)
        {
            ExitNodeCrawl((crawler, Comp<NodeCrawlerComponent>(crawler)));
        }
    }

    private void OnCrawlerShutdown(Entity<NodeCrawlerComponent> ent, ref ComponentShutdown args)
    {
        ExitNodeCrawl(ent);
    }

    private void OnCrawlableAnchorChanged(Entity<CrawlableNodeComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (args.Anchored)
            return;

        foreach (var crawler in ent.Comp.Crawlers)
        {
            var movement = Comp<NodeCrawlerMovementComponent>(crawler);
            if (movement.HeldCrawler is not { } held)
                continue;

            ExitNodeCrawl((held, Comp<NodeCrawlerComponent>(held)));
        }
    }
}
