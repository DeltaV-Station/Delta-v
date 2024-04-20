using Content.Shared.DeltaV.Tag.Components;
using Content.Shared.Tag;

namespace Content.Shared.DeltaV.Tag.Systems;

public sealed class TagOverrideSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;

    public override void Initialize()
    {
        // "DO NOT depend on the values of other components to be correct." This sign wont stop me, because I can't read!
        SubscribeLocalEvent<TagOverrideComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, TagOverrideComponent component, ComponentInit args)
    {
        if (component.AddTags != null)
            _tagSystem.AddTags(uid, component.AddTags);

        if (component.RemoveTags != null)
            _tagSystem.RemoveTags(uid, component.RemoveTags);
    }
}
