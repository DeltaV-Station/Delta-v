using Content.Shared.Paper;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// This handles preventing writing when using the illiterate trait.
/// </summary>
public sealed class IlliterateSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IlliterateComponent, ComponentStartup>(PreventWriting);
    }

    private void PreventWriting(EntityUid uid, IlliterateComponent component, ComponentStartup args)
    {
        EnsureComp<BlockWritingComponent>(uid, out var illiterateComponent);
        Dirty(uid, illiterateComponent);
    }
}
