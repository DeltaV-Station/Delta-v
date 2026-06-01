using Content.Shared.Interaction;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Utility;

namespace Content.Shared._DV.Pager;

public abstract class SharedPagerButtonSystem : EntitySystem
{
    [Dependency] private readonly PageSenderSystem _pageSender = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PagerButtonComponent, ActivateInWorldEvent>(OnActivated);
    }

    protected virtual string Location(Entity<PagerButtonComponent> ent)
    {
        return string.Empty;
    }

    protected virtual void AfterActivated(Entity<PagerButtonComponent> ent)
    {
    }

    private void OnActivated(Entity<PagerButtonComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        _audio.PlayPredicted(ent.Comp.PressSound, ent, args.User);
        _pageSender.Notify(ent.Owner, Loc.GetString(ent.Comp.PageMessage, ("location", FormattedMessage.RemoveMarkupPermissive(Location(ent)))));
        args.Handled = true;

        AfterActivated(ent);
    }
}
