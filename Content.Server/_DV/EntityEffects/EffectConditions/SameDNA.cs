using Content.Server.Forensics;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Chemistry;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server._DV.EntityEffects.EffectConditions;

/// <summary>
///     Condition that ensures the reagent being metabolised has the same DNA as the metaboliser
/// </summary>
public sealed partial class SameDNA : EntityEffectCondition
{
    public override bool Condition(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent<DnaComponent>(args.TargetEntity, out var dnaComponent))
            return false;
        if (args is not EntityEffectReagentArgs reagentArgs)
            return false;

        var expectedData = new DnaData { DNA = dnaComponent.DNA };

        return
            reagentArgs.Source is Solution solution &&
                solution.Contents
                    .Any(reagent => reagent.Reagent.Data is not null && reagent.Reagent.Data.Any(data => data == expectedData));
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-guidebook-same-dna-condition");
    }
}
