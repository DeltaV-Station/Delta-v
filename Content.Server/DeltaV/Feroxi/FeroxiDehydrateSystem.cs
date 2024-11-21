using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Nutrition.Components;

namespace Content.Server.DeltaV.Feroxi;

public sealed class FeroxiDehydrateSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;

    private const float DehydrationThreshold;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<FeroxiDehydrateComponent, ThirstComponent>();

        while (query.MoveNext(out var uid, out var feroxiDehydrate, out var thirst))
        {
            var currentThirst = thirst.CurrentThirst;
            var shouldBeDehydrated = currentThirst <= DehydrationThreshold;

            if (feroxiDehydrate.Dehydrated != shouldBeDehydrated)
            {
                UpdateDehydrationStatus(uid, feroxiDehydrate, shouldBeDehydrated);
            }
        }
    }

    private void UpdateDehydrationStatus(EntityUid uid, FeroxiDehydrateComponent feroxiDehydrate, bool shouldBeDehydrated)
    {
        feroxiDehydrate.Dehydrated = shouldBeDehydrated;

        foreach (var entity in _bodySystem.GetBodyOrganEntityComps<LungComponent>(uid))
        {
            if (TryComp<MetabolizerComponent>(entity, out var metabolizer))
            {
                var newMetabolizer = shouldBeDehydrated ? feroxiDehydrate.DehydratedMetabolizer : feroxiDehydrate.HydratedMetabolizer;
                metabolizer.MetabolizerTypes!.Clear();
                metabolizer.MetabolizerTypes.Add(newMetabolizer);
            }
        }
    }
}
