using Content.Server.DeltaV.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server.DeltaV.Objectives.Systems;

public sealed class NukeStationConditionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NukeStationConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    // TODO: Make this work!
    private void OnGetProgress(Entity<NukeStationConditionComponent> condition, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = .0f;
    }

}
