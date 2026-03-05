using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Shared.Body.Part;
using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.Inventory;
// Begin DeltaV Additions
using Content.Shared._DV.Surgery;
using Content.Shared.FixedPoint;
using Content.Shared.Forensics.Components;
using Content.Shared.Traits.Assorted;
// End DeltaV Additions
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared._Shitmed.Medical.Surgery.Conditions;
using Content.Shared._Shitmed.Medical.Surgery.Effects.Step;
using Content.Shared._Shitmed.Medical.Surgery.Tools;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Shared.Verbs;
using Content.Shared._Goobstation.CCVar;

namespace Content.Server._Shitmed.Medical.Surgery;

public sealed class SurgerySystem : SharedSurgerySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SurgeryCleanSystem _clean = default!; // DeltaV
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly InventorySystem _inventory = default!; // DeltaV - surgery cross contamination

    private readonly HashSet<string> _dirtyDnas = new(); // DeltaV

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgeryToolComponent, GetVerbsEvent<UtilityVerb>>(OnUtilityVerb);
        SubscribeLocalEvent<SurgeryTargetComponent, SurgeryStepDamageEvent>(OnSurgeryStepDamage);
        SubscribeLocalEvent<SurgeryContaminableComponent, SurgeryDirtinessEvent>(OnSurgeryDirtiness); // DeltaV
        // You might be wondering "why aren't we using StepEvent for these two?" reason being that StepEvent fires off regardless of success on the previous functions
        // so this would heal entities even if you had a used or incorrect organ.
        SubscribeLocalEvent<SurgerySpecialDamageChangeEffectComponent, SurgeryStepDamageChangeEvent>(OnSurgerySpecialDamageChange);
        SubscribeLocalEvent<SurgeryDamageChangeEffectComponent, SurgeryStepEvent>(OnSurgeryDamageChange); // DeltaV - Use SurgeryStepEvent so steps can actually damage the patient
        SubscribeLocalEvent<SurgeryStepEmoteEffectComponent, SurgeryStepEvent>(OnStepScreamComplete);
        SubscribeLocalEvent<SurgeryStepSpawnEffectComponent, SurgeryStepEvent>(OnStepSpawnComplete);
    }

    protected override void RefreshUI(EntityUid body)
    {
        var surgeries = new Dictionary<NetEntity, List<EntProtoId>>();
        foreach (var surgery in AllSurgeries)
        {
            if (GetSingleton(surgery) is not { } surgeryEnt)
                continue;

            foreach (var part in _body.GetBodyChildren(body))
            {
                var ev = new SurgeryValidEvent(body, part.Id);
                RaiseLocalEvent(surgeryEnt, ref ev);

                if (ev.Cancelled)
                    continue;

                surgeries.GetOrNew(GetNetEntity(part.Id)).Add(surgery);
            }

        }
        _ui.SetUiState(body, SurgeryUIKey.Key, new SurgeryBuiState(surgeries));
        /*
            Reason we do this is because when applying a BUI State, it rolls back the state on the entity temporarily,
            which just so happens to occur right as we're checking for step completion, so we end up with the UI
            not updating at all until you change tools or reopen the window. I love shitcode.
        */
        _ui.ServerSendUiMessage(body, SurgeryUIKey.Key, new SurgeryBuiRefreshMessage());
    }
    private void SetDamage(EntityUid body,
        DamageSpecifier damage,
        float partMultiplier,
        EntityUid user,
        EntityUid part)
    {
        if (!TryComp<BodyPartComponent>(part, out var partComp))
            return;

        _damageable.TryChangeDamage(body,
            damage,
            true,
            origin: user,
            canSever: false,
            partMultiplier: partMultiplier,
            targetPart: _body.GetTargetBodyPart(partComp));
    }

    private void AttemptStartSurgery(Entity<SurgeryToolComponent> ent, EntityUid user, EntityUid target)
    {
        if (!IsLyingDown(target, user))
            return;

        if (user == target && !_config.GetCVar(GoobCVars.CanOperateOnSelf))
        {
            _popup.PopupEntity(Loc.GetString("surgery-error-self-surgery"), user, user);
            return;
        }

        _ui.OpenUi(target, SurgeryUIKey.Key, user);
        RefreshUI(target);
    }

    private void OnUtilityVerb(Entity<SurgeryToolComponent> ent, ref GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanInteract
            || !args.CanAccess
            || !HasComp<SurgeryTargetComponent>(args.Target))
            return;

        var user = args.User;
        var target = args.Target;

        var verb = new UtilityVerb()
        {
            Act = () => AttemptStartSurgery(ent, user, target),
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Objects/Specific/Medical/Surgery/scalpel.rsi/"), "scalpel"),
            Text = Loc.GetString("surgery-verb-text"),
            Message = Loc.GetString("surgery-verb-message"),
            DoContactInteraction = true
        };

        args.Verbs.Add(verb);
    }

    private void OnSurgeryStepDamage(Entity<SurgeryTargetComponent> ent, ref SurgeryStepDamageEvent args) =>
        SetDamage(args.Body, args.Damage, args.PartMultiplier, args.User, args.Part);

    // Begin DeltaV Additions - surgery cross contamination
    public FixedPoint2 TotalDirtiness(EntityUid user, List<EntityUid> tools, Entity<DnaComponent, SurgeryContaminableComponent> target)
    {
        var total = FixedPoint2.Zero;
        _dirtyDnas.Clear();

        // TODO: make this use event(s)
        if (HasComp<SurgerySelfDirtyComponent>(user))
        {
            total += _clean.Dirtiness(user);
            _dirtyDnas.UnionWith(_clean.CrossContaminants(user));
        }
        else
        {
            if (_inventory.TryGetSlotEntity(user, "gloves", out var glovesEntity))
            {
                total += _clean.Dirtiness(glovesEntity.Value);
                _dirtyDnas.UnionWith(_clean.CrossContaminants(glovesEntity.Value));
            }

            foreach (var tool in tools)
            {
                total += _clean.Dirtiness(tool);
                _dirtyDnas.UnionWith(_clean.CrossContaminants(tool));
            }
        }

        if (target.Comp1.DNA is {} dna)
            _dirtyDnas.Remove(dna);

        return total + _dirtyDnas.Count * target.Comp2.CrossContaminationDirtinessLevel;
    }

    public FixedPoint2 DamageToBeDealt(Entity<SurgeryContaminableComponent> ent, FixedPoint2 dirtiness)
    {
        if (ent.Comp.DirtinessThreshold > dirtiness)
            return 0;

        var exceedsAmount = (dirtiness - ent.Comp.DirtinessThreshold).Float();
        var additionalDamage = (1f / ent.Comp.InverseDamageCoefficient.Float()) * (exceedsAmount * exceedsAmount);

        return FixedPoint2.Min(FixedPoint2.New(additionalDamage) + ent.Comp.BaseDamage, ent.Comp.ToxinStepLimit);
    }

    private void OnSurgeryDirtiness(Entity<SurgeryContaminableComponent> ent, ref SurgeryDirtinessEvent args)
    {
        if (!TryComp<DnaComponent>(ent, out var dnaComp))
            return;

        var dirtiness = TotalDirtiness(args.User, args.Tools, (ent, dnaComp, ent));
        var damage = DamageToBeDealt(ent, dirtiness);

        if (damage > 0)
        {
            var sepsis = new DamageSpecifier(_prototypes.Index(ent.Comp.SepsisDamageType), damage);
            SetDamage(ent, sepsis, 0.5f, args.User, args.Part);
        }

        if (!TryComp<SurgeryStepDirtinessComponent>(args.Step, out var surgicalStepDirtiness))
            return;

        if (HasComp<SurgerySelfDirtyComponent>(args.User))
        {
            _clean.AddDirt(args.User, surgicalStepDirtiness.ToolDirtiness);
            _clean.AddDna(args.User, dnaComp.DNA);
            return;
        }

        if (_inventory.TryGetSlotEntity(args.User, "gloves", out var glovesEntity))
        {
            _clean.AddDirt(glovesEntity.Value, surgicalStepDirtiness.GloveDirtiness);
            _clean.AddDna(glovesEntity.Value, dnaComp.DNA);
        }
        foreach (var tool in args.Tools)
        {
            // don't dirty random non-surgery items like soap or clothes
            // ideally Tools would only contain the (highest quality) tool used for the step
            if (!HasComp<SurgeryToolComponent>(tool))
                continue;

            _clean.AddDirt(tool, surgicalStepDirtiness.ToolDirtiness);
            _clean.AddDna(tool, dnaComp.DNA);
        }
    }
    // End DeltaV Additions

    private void OnSurgeryDamageChange(Entity<SurgeryDamageChangeEffectComponent> ent, ref SurgeryStepEvent args) // DeltaV
    {
        var damageChange = ent.Comp.Damage;
        if (HasComp<AnesthesiaComponent>(args.Body) || _mobState.IsDead(args.Body)) // DeltaV - anesthesia
            damageChange = damageChange * ent.Comp.SleepModifier;

        SetDamage(args.Body, damageChange, 0.5f, args.User, args.Part);
    }

    private void OnSurgerySpecialDamageChange(Entity<SurgerySpecialDamageChangeEffectComponent> ent, ref SurgeryStepDamageChangeEvent args)
    {
        /* DeltaV - this shit was killed
        // Im killing this shit soon too, inshallah.
        if (ent.Comp.DamageType == "Rot")
             _rot.ReduceAccumulator(args.Body, TimeSpan.FromSeconds(2147483648)); // BEHOLD, SHITCODE THAT I JUST COPY PASTED. I'll redo it at some point, pinky swear :)
        */
        /*else if (ent.Comp.DamageType == "Eye"
            && TryComp(ent, out BlindableComponent? blindComp)
            && blindComp.EyeDamage > 0)
            _blindableSystem.AdjustEyeDamage((args.Body, blindComp), -blindComp!.EyeDamage);*/
    }

    private void OnStepScreamComplete(Entity<SurgeryStepEmoteEffectComponent> ent, ref SurgeryStepEvent args)
    {
        if (HasComp<AnesthesiaComponent>(args.Body)) // DeltaV
            return;
        if (HasComp<PainNumbnessComponent>(args.Body)) // DeltaV
            return;

        _chat.TryEmoteWithChat(args.Body, ent.Comp.Emote);
    }
    private void OnStepSpawnComplete(Entity<SurgeryStepSpawnEffectComponent> ent, ref SurgeryStepEvent args) =>
        SpawnAtPosition(ent.Comp.Entity, Transform(args.Body).Coordinates);
}
