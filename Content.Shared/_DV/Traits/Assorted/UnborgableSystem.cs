using Content.Shared.Body.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Systems;
using Content.Shared.Examine;
using Content.Shared.Movement.Components; // TODO: use BrainComponent instead of InputMover when shitmed is merged
using Robust.Shared.Utility;

namespace Content.Shared._DV.Traits.Assorted;

/// <summary>
/// Adds a warning examine message to brains with <see cref="UnborgableComponent"/>.
/// </summary>
public sealed class UnborgableSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UnborgableComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<UnborgableComponent, ExaminedEvent>(OnExamined);
    }

    /// <summary>
    /// Returns true if a mob's brain has <see cref="UnborgableComponent"/>.
    /// </summary>
    public bool IsUnborgable(Entity<BodyComponent?> ent)
    {
        // technically this will apply for any organ not just brain, but assume nobody will be evil and do that
        return _body.GetBodyOrganEntityComps<UnborgableComponent>(ent).Count > 0;
    }

    private void OnMapInit(Entity<UnborgableComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<BodyComponent>(ent, out var body))
            return;

        var brains = _body.GetBodyOrganEntityComps<InputMoverComponent>((ent.Owner, body));
        foreach (var brain in brains)
        {
            EnsureComp<UnborgableComponent>(brain);
        }
    }

    private void OnExamined(Entity<UnborgableComponent> ent, ref ExaminedEvent args)
    {
        // need a health analyzer to see if someone can't be borged, can't just look at them and know
        if (!args.IsInDetailsRange || HasComp<BodyComponent>(ent))
            return;

        args.PushMarkup(Loc.GetString("brain-cannot-be-borged-message"));
    }
}
