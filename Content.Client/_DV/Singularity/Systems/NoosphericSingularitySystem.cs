using Content.Shared._DV.Singularity.Components;
using Content.Shared._DV.Singularity.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Client._DV.Singularity.Systems;

/// <summary>
/// The client-side version of <see cref="SharedNoosphericSingularitySystem"/>.
/// Primarily manages <see cref="NoosphericSingularityComponent"/>s.
/// </summary>
public sealed class SingularitySystem : SharedNoosphericSingularitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NoosphericSingularityComponent, ComponentHandleState>(HandleSingularityState);
    }

    /// <summary>
    /// Handles syncing singularities with their server-side versions.
    /// </summary>
    /// <param name="uid">The uid of the singularity to sync.</param>
    /// <param name="comp">The state of the singularity to sync.</param>
    /// <param name="args">The event arguments including the state to sync the singularity with.</param>
    private void HandleSingularityState(Entity<NoosphericSingularityComponent> ent, ref ComponentHandleState args)
    {
        if (args.Current is not NoosphericSingularityComponentState state)
            return;

        SetLevel(ent, state.Level, ent.Comp);
    }
}
