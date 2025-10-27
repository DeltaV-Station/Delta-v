using System.Linq;
using Content.Shared.Chemistry;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Xenoarchaeology.Artifact.Components;
using Content.Shared.Xenoarchaeology.Artifact.XAT.Components;

namespace Content.Shared.Xenoarchaeology.Artifact;

public abstract partial class SharedXenoArtifactSystem
{
    private void InitializeXAT()
    {
        XATRelayLocalEvent<DamageChangedEvent>();
        XATRelayLocalEvent<InteractUsingEvent>();
        XATRelayLocalEvent<PullStartedMessage>();
        XATRelayLocalEvent<AttackedEvent>();
        XATRelayLocalEvent<XATToolUseDoAfterEvent>();
        XATRelayLocalEvent<InteractHandEvent>();
        XATRelayLocalEvent<ReactionEntityEvent>();
        XATRelayLocalEvent<LandEvent>();

        // special case this one because we need to order the messages
        SubscribeLocalEvent<XenoArtifactComponent, ExaminedEvent>(OnExamined);
    }

    /// <summary> Relays artifact events for artifact nodes. </summary>
    protected void XATRelayLocalEvent<T>() where T : notnull
    {
        SubscribeLocalEvent<XenoArtifactComponent, T>(RelayEventToNodes);
    }

    private void OnExamined(Entity<XenoArtifactComponent> ent, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(XenoArtifactComponent)))
        {
            RelayEventToNodes(ent, ref args);
        }
    }

    protected void RelayEventToNodes<T>(Entity<XenoArtifactComponent> ent, ref T args) where T : notnull
    {
        var ev = new XenoArchNodeRelayedEvent<T>(ent, args);

        var nodes = GetAllNodes(ent);
        foreach (var node in nodes)
        {
            RaiseLocalEvent(node, ref ev);
        }
    }

    /// <summary>
    /// Attempts to shift artifact into unlocking state, in which it is going to listen to interactions, that could trigger nodes.
    /// </summary>
    public void TriggerXenoArtifact(Entity<XenoArtifactComponent> ent, Entity<XenoArtifactNodeComponent>? node, bool force = false)
    {
        // limits spontaneous chain activations, also prevents spamming every triggering tool to activate nodes
        // without real knowledge about triggers
        if (!force && _timing.CurTime < ent.Comp.NextUnlockTime)
            return;

        // DeltaV - start of node scanner overhaul
        (Entity<XenoArtifactNodeComponent> node, int index)? parsedNode = 
            (node == null) 
            ? null 
            : (node.Value, GetIndex(ent, node.Value));

        bool partOfRelatedTriggersSet = true;
        // DeltaV - end of node scanner overhaul

        if (!_unlockingQuery.TryGetComponent(ent, out var unlockingComp))
        {
            unlockingComp = EnsureComp<XenoArtifactUnlockingComponent>(ent);
            unlockingComp.EndTime = _timing.CurTime + ent.Comp.UnlockStateDuration;
            Log.Debug($"{ToPrettyString(ent)} entered unlocking state");

            if (_net.IsServer)
                _popup.PopupEntity(Loc.GetString("artifact-unlock-state-begin"), ent);
            Dirty(ent);
        }
        else if (parsedNode != null)
        {
            // DeltaV - start of node scanner overhaul

            var relatedNodeIndices = GetRelatedNodes((ent, ent), parsedNode.Value.index);
            partOfRelatedTriggersSet = unlockingComp.TriggeredNodeIndexesRelated.All(x => relatedNodeIndices.Contains(x));

            // Checking for trigger "relatedness" is a much more accurate measurement 
            //   of "is this locking phase going to fail" than upstream's predecessor/successor check.
            // Upstream's version of this check had edge-cases where time would not add, even though the
            //   unlocking phase ends up succeeding.
            // See definition of GetRelatedNodes() for details on the concept of "relatedness".
            if (
                unlockingComp.TriggeredNodeIndexes.Count == unlockingComp.TriggeredNodeIndexesRelated.Count 
                && partOfRelatedTriggersSet
            )
                // we add time on each new trigger, if it is not going to fail us
                unlockingComp.EndTime += ent.Comp.UnlockStateIncrementPerNode;

            // DeltaV - end of node scanner overhaul
        }

        if (parsedNode != null && unlockingComp.TriggeredNodeIndexes.Add(parsedNode.Value.index))
        {
            // DeltaV - start of changes
            // node scanner overhaul:
            unlockingComp.TriggeredNodeIndexesOrdered.Add(parsedNode.Value.index);
            if (partOfRelatedTriggersSet)
                unlockingComp.TriggeredNodeIndexesRelated.Add(parsedNode.Value.index);

            // faster unlock effect:
            if (
                ent.Comp.UnlockCompleteDuration is {} completeDuration 
                && TryGetNodeFromUnlockState((ent.Owner, unlockingComp, ent.Comp), out var unlockingNode)
            )
            {
                unlockingComp.EndTime = _timing.CurTime + completeDuration;
            }
            // DeltaV - end of changes

            Dirty(ent, unlockingComp);
        }
    }

    public void SetArtifexiumApplied(Entity<XenoArtifactUnlockingComponent> ent, bool val)
    {
        ent.Comp.ArtifexiumApplied = val;
        Dirty(ent);
    }
}

/// <summary>
/// Event wrapper for XenoArch Trigger events.
/// </summary>
[ByRefEvent]
public record struct XenoArchNodeRelayedEvent<TEvent>(Entity<XenoArtifactComponent> Artifact, TEvent Args)
{
    /// <summary>
    /// Original event.
    /// </summary>
    public TEvent Args = Args;

    /// <summary>
    /// Artifact entity, that received original event.
    /// </summary>
    public Entity<XenoArtifactComponent> Artifact = Artifact;
}
