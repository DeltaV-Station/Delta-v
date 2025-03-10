using Content.Shared.Paper;

namespace Content.Shared._DV.Traits.Assorted;

/// <summary>
/// This handles preventing writing when using the dysgraphia trait.
/// </summary>
public sealed class DysgraphiaSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DysgraphiaComponent, ComponentStartup>(PreventWriting);
    }

    private void PreventWriting(EntityUid uid, DysgraphiaComponent component, ComponentStartup args)
    {
        EnsureComp<BlockWritingComponent>(uid);
    }
}
