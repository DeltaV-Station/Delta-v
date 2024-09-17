using System.Diagnostics;
using Content.Shared.Nutrition.Components;

namespace Content.Server.DeltaV.Feroxi;

public sealed class FeroxiDehydrateSystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<FeroxiDehydrateComponent, ThirstComponent>();

        while (query.MoveNext(out var uid, out var feroxiDehydrate, out var thirst))
        {
            if(thirst.CurrentThirstThreshold != thirst.LastThirstThreshold)
            {

            }
        }
    }
}

