using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Tips.Conditions;

/// <summary>
/// Condition that checks if the player entity has a specific component.
/// </summary>
public sealed partial class HasCompCondition : TipCondition
{
    private ISawmill _sawmill = default!;

    /// <summary>
    /// The component name to check for.
    /// </summary>
    [DataField(required: true, customTypeSerializer: typeof(ComponentNameSerializer))]
    public string Comp = string.Empty;

    protected override bool EvaluateImplementation(TipConditionContext ctx)
    {
        _sawmill = ctx.LogMan.GetSawmill("HasCompCondition");
        if (!ctx.CompFactory.TryGetRegistration(Comp, out var registration))
        {
            _sawmill.Warning("tip", $"Tip condition references unknown component: {Comp}");
            return false;
        }

        return ctx.EntMan.HasComponent(ctx.Player, registration.Type);
    }
}
