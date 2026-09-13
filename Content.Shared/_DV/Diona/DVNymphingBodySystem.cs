using Content.Shared.Gibbing;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Species;

namespace Content.Shared._DV.Diona;

public sealed class DVNymphingBodySystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly GibbingSystem _gibbing = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly DVNymphRelationSystem _nymph = default!;

	public override void Initialize()
	{
		base.Initialize();

		SubscribeLocalEvent<DVNymphingBodyComponent, GibActionSystem.GibActionEvent>(OnNymphingBodyGib);
        SubscribeLocalEvent<DVNymphingBodyComponent, GibbedBeforeDeletionEvent>(OnGibbedBeforeDeletion);
	}

    private void OnNymphingBodyGib(Entity<DVNymphingBodyComponent> ent, ref GibActionSystem.GibActionEvent args)
    {
        _popup.PopupPredicted(Loc.GetString(ent.Comp.PopupText, ("name", ent)), ent, ent);
        _gibbing.Gib(ent, user: args.Performer);
    }

    private void OnGibbedBeforeDeletion(Entity<DVNymphingBodyComponent> ent, ref GibbedBeforeDeletionEvent args)
    {
        EntityUid? leadGiblet = null;
        var leadMind = EntityUid.Invalid;

        foreach (var giblet in args.Giblets)
        {
            if (!_mind.TryGetMind(giblet, out leadMind, out _))
                continue;

            leadGiblet = giblet;
            break;
        }

        if (leadGiblet is not { } leader || !TryComp<DVNymphLeadComponent>(leader, out var leaderComp))
            return;

        if (TryComp<DVNymphMindMemoryComponent>(leader, out var memory) && Exists(leadMind))
        {
            memory.Mind = leadMind;
        }

        foreach (var giblet in args.Giblets)
        {
            if (!TryComp<DVNymphFollowerComponent>(giblet, out var follower))
                continue;

            _nymph.Follow((leader, leaderComp), (giblet, follower));
        }
    }

}
