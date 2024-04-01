using Content.Shared.DeltaV.SpaceFerret;
using Content.Shared.Objectives.Components;

namespace Content.Server.DeltaV.SpaceFerret;

public sealed class ConsumeNutrientsObjectiveSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConsumeNutrientsConditionComponent, ObjectiveGetProgressEvent>(OnConsumeNutrientsGetProgress);
    }

    private static void OnConsumeNutrientsGetProgress(EntityUid uid, ConsumeNutrientsConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = comp.NutrientsConsumed / comp.NutrientsRequired;
    }
}
