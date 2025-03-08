using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Utility;
using Content.Shared.FixedPoint;

namespace Content.Shared._DV.Surgery;

public abstract class SharedSurgeryCleanSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgeryCleansDirtComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<SurgeryCleansDirtComponent, SurgeryCleanDirtDoAfterEvent>(FinishCleaning);

        SubscribeLocalEvent<SurgeryDirtinessComponent, ExaminedEvent>(OnDirtyExamined);
        SubscribeLocalEvent<SurgeryCleansDirtComponent, GetVerbsEvent<UtilityVerb>>(OnUtilityVerb);

        SubscribeLocalEvent<SurgeryDirtinessComponent, SurgeryCleanedEvent>(OnCleanDirt);
    }

    private void OnUtilityVerb(Entity<SurgeryCleansDirtComponent> ent, ref GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var user = args.User;
        var target = args.Target;

        var verb = new UtilityVerb()
        {
            Act = () => TryStartCleaning(ent, user, target),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/bubbles.svg.192dpi.png")),
            Text = Loc.GetString(Loc.GetString("sanitization-verb-text")),
            Message = Loc.GetString(Loc.GetString("sanitization-verb-message")),
            // we daisychain to forensics here so we shouldn't leave forensic traces of our own
            DoContactInteraction = false
        };

        args.Verbs.Add(verb);
    }

    private void OnDirtyExamined(Entity<SurgeryDirtinessComponent> ent, ref ExaminedEvent args)
    {
        var stage = (int)Math.Ceiling(ent.Comp.Dirtiness.Double() / 20.0);

        var description = stage switch {
            0 => "surgery-cleanliness-0",
            1 => "surgery-cleanliness-1",
            2 => "surgery-cleanliness-2",
            3 => "surgery-cleanliness-3",
            4 => "surgery-cleanliness-4",
            _ => "surgery-cleanliness-5",
        };

        args.PushMarkup(Loc.GetString(description));
    }

    public abstract bool RequiresCleaning(EntityUid target);

    private void OnAfterInteract(Entity<SurgeryCleansDirtComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        args.Handled = TryStartCleaning(ent, args.User, args.Target.Value);
    }

    public bool TryStartCleaning(Entity<SurgeryCleansDirtComponent> ent, EntityUid user, EntityUid target)
    {
        if (!RequiresCleaning(target))
        {
            _popup.PopupPredicted(Loc.GetString("sanitization-cannot-clean", ("target", target)), user, user, PopupType.MediumCaution);
            return false;
        }

        var cleanDelay = ent.Comp.CleanDelay;
        var doAfterArgs = new DoAfterArgs(EntityManager, user, cleanDelay, new SurgeryCleanDirtDoAfterEvent(), ent, target: target, used: ent)
        {
            NeedHand = true,
            BreakOnDamage = true,
            BreakOnMove = true,
            MovementThreshold = 0.01f,
            DistanceThreshold = 1f,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        _popup.PopupClient(Loc.GetString("sanitization-cleaning", ("target", target)), user, user);

        return true;
    }

    protected virtual void FinishCleaning(Entity<SurgeryCleansDirtComponent> ent, ref SurgeryCleanDirtDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Args.Target == null)
            return;

        DoClean(ent, args.Args.Target.Value);

        args.Repeat = RequiresCleaning(args.Args.Target.Value);
    }

    public void DoClean(Entity<SurgeryCleansDirtComponent> cleaner, EntityUid target)
    {
        var ev = new SurgeryCleanedEvent(cleaner.Comp.DirtAmount, cleaner.Comp.DnaAmount);
        RaiseLocalEvent(target, ref ev);
    }

    private void OnCleanDirt(Entity<SurgeryDirtinessComponent> ent, ref SurgeryCleanedEvent args)
    {
        ent.Comp.Dirtiness = FixedPoint2.Max(FixedPoint2.Zero, ent.Comp.Dirtiness - args.DirtAmount);
        Dirty(ent);
    }
}
