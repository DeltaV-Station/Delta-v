using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Examine;
using Content.Shared.Movement.Components; // TODO: use BrainComponent instead of InputMover if it gets moved to shared

namespace Content.Shared._DV.Traits.Assorted;

/// <summary>
/// Adds a warning examine message to brains with <see cref="UnborgableComponent"/>.
/// </summary>
public sealed class UnborgableSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;

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
        if (ent.Comp == null || ent.Comp.Organs == null)
            return false;

        foreach (var organ in ent.Comp.Organs.ContainedEntities)
        {
            if (HasComp<UnborgableComponent>(organ))
                return true;
        }
        return false;
    }

    private void OnMapInit(Entity<UnborgableComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<BodyComponent>(ent, out var body))
            return;

        var ev = new MakeBrainUnborgableEvent();
        _body.RelayEvent((ent, body), ref ev);
    }

    private void OnExamined(Entity<UnborgableComponent> ent, ref ExaminedEvent args)
    {
        // need a health analyzer to see if someone can't be borged, can't just look at them and know
        if (!args.IsInDetailsRange || HasComp<BodyComponent>(ent))
            return;

        args.PushMarkup(Loc.GetString("brain-cannot-be-borged-message"));
    }
}
