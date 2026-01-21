using Content.Server._DV.CosmicCult.Components;
using Content.Server.Bible.Components;
using Content.Server.Popups;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult;
using Content.Shared.Damage.Systems;
using Content.Shared.Mind;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicConversionSystem : EntitySystem
{
    [Dependency] private readonly CosmicCultRuleSystem _cultRule = default!;
    [Dependency] private readonly CosmicGlyphSystem _cosmicGlyph = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedCosmicCultSystem _cosmicCult = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicGlyphConversionComponent, TryActivateGlyphEvent>(OnConversionGlyph);
        SubscribeLocalEvent<CosmicGlyphConversionComponent, CheckGlyphConditionsEvent>(OnCheckGlyphConditions);
    }

    private void OnCheckGlyphConditions(Entity<CosmicGlyphConversionComponent> uid, ref CheckGlyphConditionsEvent args)
    {
        var possibleTargets = _cosmicGlyph.GetTargetsNearGlyph(uid, uid.Comp.ConversionRange, entity => _cosmicCult.EntityIsCultist(entity));
        if (possibleTargets.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("cult-glyph-conditions-not-met"), uid, args.User);
            args.Cancel();
            return;
        }
        if (possibleTargets.Count > 1)
        {
            _popup.PopupEntity(Loc.GetString("cult-glyph-too-many-targets"), uid, args.User);
            args.Cancel();
            return;
        }
        foreach (var target in possibleTargets)
        {
            if (!_mind.TryGetMind(target, out _, out _))
            {
                _popup.PopupEntity(Loc.GetString("cult-glyph-target-mindless"), uid, args.User);
                args.Cancel();
                return;
            }
        }
    }

    private void OnConversionGlyph(Entity<CosmicGlyphConversionComponent> uid, ref TryActivateGlyphEvent args)
    {
        var ev = new CheckGlyphConditionsEvent(args.User, args.Cultists);
        RaiseLocalEvent(uid, ref ev);
        if (ev.Cancelled)
        {
            args.Cancel();
            return;
        }

        var possibleTargets = _cosmicGlyph.GetTargetsNearGlyph(uid, uid.Comp.ConversionRange, entity => _cosmicCult.EntityIsCultist(entity));

        foreach (var target in possibleTargets)
        {
            if (_mobState.IsDead(target))
            {
                _popup.PopupEntity(Loc.GetString("cult-glyph-target-dead"), uid, args.User);
                args.Cancel();
            }
            else if (uid.Comp.NegateProtection == false && HasComp<BibleUserComponent>(target))
            {
                _popup.PopupEntity(Loc.GetString("cult-glyph-target-chaplain"), uid, args.User);
                args.Cancel();
            }
            else if (uid.Comp.NegateProtection == false && HasComp<MindShieldComponent>(target))
            {
                _popup.PopupEntity(Loc.GetString("cult-glyph-target-mindshield"), uid, args.User);
                args.Cancel();
            }
            else
            {
                _stun.TryUpdateStunDuration(target, TimeSpan.FromSeconds(4f));
                _damageable.TryChangeDamage(target.Owner, uid.Comp.ConversionHeal * -1);
                _cultRule.CosmicConversion(uid, target);
            }
        }
    }
}
