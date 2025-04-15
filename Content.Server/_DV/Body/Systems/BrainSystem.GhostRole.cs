using Content.Server.Body.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Mind.Components;
using Robust.Shared.Containers;

namespace Content.Server.Body.Systems;

/// <summary>
/// Handles transferring mind to the body when a installed brain's ghost role is taken.
/// Currently this is only used for positronic brains in IPCs.
/// </summary>
public sealed partial class BrainSystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private void InitializeGhostRole()
    {
        SubscribeLocalEvent<BrainComponent, MindAddedMessage>(OnMindAdded);
    }

    private void OnMindAdded(Entity<BrainComponent> ent, ref MindAddedMessage args)
    {
        // the brain needs to be installed in a body
        if (!TryComp<OrganComponent>(ent, out var organ) || organ.Body is not {} body)
            return;

        HandleMind(body, ent, ent);
    }
}
