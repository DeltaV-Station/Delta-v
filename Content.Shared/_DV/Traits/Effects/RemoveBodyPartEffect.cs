using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;

namespace Content.Shared._DV.Traits.Effects;

/// <summary>
/// Effect that removes body parts of the player body.
/// </summary>
public sealed partial class RemoveBodyPartEffect : BaseTraitEffect
{

    [DataField(required: true)]
    public BodyPartType Part = default;

    [DataField(required: true)]
    public BodyPartSymmetry Symmetry = default!;

    public override void Apply(TraitEffectContext ctx)
    {
        if (!ctx.EntMan.TryGetComponent(ctx.Player, out BodyComponent? body))
            return;

        if (!ctx.EntMan.TrySystem<SharedBodySystem>(out var bodySys))
            return;

        if (!ctx.EntMan.TrySystem<SharedBloodstreamSystem>(out var bloodstreamSys))
            return;

        if (bodySys.GetRootPartOrNull(ctx.Player, body) is not { } root)
            return;

        foreach (var part in bodySys.GetBodyChildrenOfType(ctx.Player, Part, body, Symmetry))
        {
            foreach (var child in bodySys.GetBodyPartChildren(part.Id, part.Component))
            {
                ctx.EntMan.DeleteEntity(child.Id);
            }
            ctx.EntMan.DeleteEntity(part.Id);

            // apparently chopping off limbs makes people bleed a lot. Who would have guessed?
            bloodstreamSys.TryModifyBleedAmount(ctx.Player, -100f);
        }
    }
}
