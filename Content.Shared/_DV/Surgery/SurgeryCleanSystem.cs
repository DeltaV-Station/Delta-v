using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Forensics;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Utility;
using Content.Shared.FixedPoint;

namespace Content.Shared._DV.Surgery;

public sealed class SurgeryCleanSystem : EntitySystem
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
        SubscribeLocalEvent<SurgeryCrossContaminationComponent, SurgeryCleanedEvent>(OnCleanDNA);
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

    // no separate examine for DNAs as they are both cleaned at the same time.
    private void OnDirtyExamined(Entity<SurgeryDirtinessComponent> ent, ref ExaminedEvent args)
    {
        var stage = (int)Math.Ceiling(ent.Comp.Dirtiness.Double() / 20.0);

        // dirtiness -> stage ranges from 0.0 -> 0 to 100.0 -> 5
        args.PushMarkup(Loc.GetString($"surgery-cleanliness-{stage}"));
    }

    public bool RequiresCleaning(EntityUid target)
    {
        var isDirty = (TryComp<SurgeryDirtinessComponent>(target, out var dirtiness) && dirtiness.Dirtiness > 0);
        var isContaminated = (TryComp<SurgeryCrossContaminationComponent>(target, out var contamination) && contamination.DNAs.Count > 0);
        return isDirty || isContaminated;
    }

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

    private void FinishCleaning(Entity<SurgeryCleansDirtComponent> ent, ref SurgeryCleanDirtDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Args.Target is not {} target)
            return;

        DoClean(ent, target);

        args.Repeat = RequiresCleaning(target);

        // daisychain to forensics because if you sterilise something youve almost definitely scrubbed all dna and fibers off of it
        var daisyChainEvent = new CleanForensicsDoAfterEvent() { DoAfter = args.DoAfter };
        RaiseLocalEvent(ent, daisyChainEvent);
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

    private void OnCleanDNA(Entity<SurgeryCrossContaminationComponent> ent, ref SurgeryCleanedEvent args)
    {
        var i = 0;
        var count = args.DnaAmount;
        ent.Comp.DNAs.RemoveWhere(item => i++ < count);
        Dirty(ent);
    }

    #region Public API

    /// <summary>
    /// Get the dirtiness level for a tool/user, adding SurgeryDirtiness if it doesn't exist already.
    /// </summary>
    public FixedPoint2 Dirtiness(EntityUid uid)
    {
        return EnsureComp<SurgeryDirtinessComponent>(uid).Dirtiness;
    }

    /// <summary>
    /// Get all DNA strings contaminating a tool/user, adding SurgeryCrossContamination if it doesn't exist already.
    /// </summary>
    public HashSet<string> CrossContaminants(EntityUid uid)
    {
        return EnsureComp<SurgeryCrossContaminationComponent>(uid).DNAs;
    }

    /// <summary>
    /// Add dirt to a tool/user.
    /// </summary>
    public void AddDirt(EntityUid uid, FixedPoint2 amount)
    {
        var comp = EnsureComp<SurgeryDirtinessComponent>(uid);
        comp.Dirtiness += amount * 0.1f;
        Dirty(uid, comp);
    }

    /// <summary>
    /// Contaminate a tool/user with DNA.
    /// </summary>
    public void AddDna(EntityUid uid, string? dna)
    {
        if (dna == null)
            return;

        var comp = EnsureComp<SurgeryCrossContaminationComponent>(uid);
        comp.DNAs.Add(dna);
        Dirty(uid, comp);
    }

    #endregion
}
