using Content.Server.Ghost;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;

namespace Content.Server._DV.Psionics.Systems.PsionicPowers;

public sealed class MindSwapPowerSystem : SharedMindSwapPowerSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhostAttemptHandleEvent>(OnGhostAttempt);
    }

    private void OnGhostAttempt(GhostAttemptHandleEvent args)
    {
        if (args.Handled)
            return;

        // If you're able to swap back to your original body, you should swap back before you ghost.
        if (TryComp<MindSwappedReturnPowerComponent>(args.Mind.CurrentEntity, out var component)
            && Action.GetAction(component.ActionEntity) is { } action
            && action.Comp.AttachedEntity is not null)
        {
            args.Result = false;
            args.Handled = true;
        }
    }
}
