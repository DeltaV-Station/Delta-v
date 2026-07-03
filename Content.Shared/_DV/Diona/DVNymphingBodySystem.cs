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
	}

    private void OnNymphingBodyGib(Entity<DVNymphingBodyComponent> ent, ref GibActionSystem.GibActionEvent args)
    {
        _popup.PopupPredicted(Loc.GetString(ent.Comp.PopupText, ("name", ent)), ent, ent);
        var giblets = _gibbing.Gib(ent, user: args.Performer);
        EntityUid? leadGiblet = null;

        foreach (var giblet in giblets)
        {
            if (!_mind.TryGetMind(giblet, out var mindUid, out var mind))
                continue;

            leadGiblet = giblet;
            break;
        }

        if (leadGiblet is not { } leader || !TryComp<DVNymphLeadComponent>(leader, out var leaderComp))
            return;

        foreach (var giblet in giblets)
        {
            if (!TryComp<DVNymphFollowerComponent>(giblet, out var follower))
                continue;

            _nymph.Follow((leader, leaderComp), (giblet, follower));
        }
    }
}
