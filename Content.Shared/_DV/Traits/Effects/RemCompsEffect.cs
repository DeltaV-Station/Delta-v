using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Shared._DV.Traits.Effects;

/// <summary>
/// Effect that removes components from the player entity if they exist.
/// </summary>
public sealed partial class RemCompsEffect : BaseTraitEffect
{
    /// <summary>
    /// The component names to remove from the entity.
    /// </summary>
    [DataField(required: true, customTypeSerializer: typeof(CustomHashSetSerializer<string, ComponentNameSerializer>))]
    public HashSet<string> Components = new();

    public override void Apply(TraitEffectContext ctx)
    {
        foreach (var compName in Components)
        {
            if (!ctx.CompFactory.TryGetRegistration(compName, out var registration))
            {
                var sawmill = ctx.LogMan.GetSawmill("traits");
                sawmill.Warning($"RemCompsEffect references unknown component: {compName}");
                continue;
            }

            ctx.EntMan.RemoveComponent(ctx.Player, registration.Type);
        }
    }
}
