using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Body.Prototypes;
using Content.Shared.Nutrition.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.DeltaV.Feroxi;

public sealed class FeroxiDehydrateSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _bodySystem = default!;
    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<FeroxiDehydrateComponent, ThirstComponent>();

        while (query.MoveNext(out var uid, out var feroxiDehydrate, out var thirst))
        {
            if (thirst.CurrentThirstThreshold == thirst.LastThirstThreshold)
            {
                return;
            }
            foreach (var entity in _bodySystem.GetBodyOrganEntityComps<LungComponent>(uid))
            {
                if (!TryComp<MetabolizerComponent>(entity, out var metabolizer))
                {
                    return;
                }
                if (thirst.CurrentThirst <= thirst.ThirstThresholds[ThirstThreshold.Parched] && feroxiDehydrate.Dehydrated == false)
                {
                    feroxiDehydrate.Dehydrated = true;
                    metabolizer.MetabolizerTypes = new HashSet<ProtoId<MetabolizerTypePrototype>>();
                    metabolizer.MetabolizerTypes.Add(feroxiDehydrate.DehydratedMetabolizer);
                }
                if (thirst.CurrentThirst > thirst.ThirstThresholds[ThirstThreshold.Parched] && feroxiDehydrate.Dehydrated == true)
                {
                    feroxiDehydrate.Dehydrated = false;
                    metabolizer.MetabolizerTypes = new HashSet<ProtoId<MetabolizerTypePrototype>>();
                    metabolizer.MetabolizerTypes.Add(feroxiDehydrate.HydratedMetabolizer);
                }
            }
        }
    }
}

