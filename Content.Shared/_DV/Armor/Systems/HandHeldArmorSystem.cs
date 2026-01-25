using Content.Shared._DV.Armor.Components;
using Content.Shared.Armor;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.Hands;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Robust.Shared.Utility;

namespace Content.Shared._DV.Armor.Systems;

/// <summary>
///     This handles logic relating to <see cref="HeldArmorComponent" />
/// </summary>
public sealed class HandHeldArmorSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HandHeldArmorComponent, GotEquippedHandEvent>(OnHandEquipped);
        SubscribeLocalEvent<HandHeldArmorComponent, HeldRelayedEvent<CoefficientQueryEvent>>(OnCoefficientQuery);
        SubscribeLocalEvent<HandHeldArmorComponent, HeldRelayedEvent<DamageModifyEvent>>(OnDamageModify);
        SubscribeLocalEvent<HandHeldArmorComponent, GetVerbsEvent<ExamineVerb>>(OnHeldArmorVerbExamine);
    }

    private void OnHandEquipped(Entity<HandHeldArmorComponent> armor, ref GotEquippedHandEvent args)
    {
        armor.Comp.Holder = args.User; // So we can check the whitelists later. The events actually don't carry over the user.
    }

    private void OnCoefficientQuery(Entity<HandHeldArmorComponent> armor, ref HeldRelayedEvent<CoefficientQueryEvent> args)
    {
        if (!armor.Comp.Holder.HasValue
            || _whitelist.IsWhitelistFail(armor.Comp.Whitelist, armor.Comp.Holder.Value) // If they pass the lists, add the coefficients.
            || _whitelist.IsBlacklistPass(armor.Comp.Blacklist, armor.Comp.Holder.Value))
            return;

        foreach (var armorCoefficient in armor.Comp.Modifiers.Coefficients)
        {
            args.Args.DamageModifiers.Coefficients[armorCoefficient.Key] =
                args.Args.DamageModifiers.Coefficients.TryGetValue(armorCoefficient.Key, out var coefficient)
                ? coefficient * armorCoefficient.Value
                : armorCoefficient.Value;
        }
    }

    private void OnDamageModify(Entity<HandHeldArmorComponent> armor, ref HeldRelayedEvent<DamageModifyEvent> args)
    {
        if (!armor.Comp.Holder.HasValue
            || _whitelist.IsWhitelistFail(armor.Comp.Whitelist, armor.Comp.Holder.Value) // If they pass the lists, add the coefficients.
            || _whitelist.IsBlacklistPass(armor.Comp.Blacklist, armor.Comp.Holder.Value))
            return;

        args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, armor.Comp.Modifiers);
    }

    private void OnHeldArmorVerbExamine(Entity<HandHeldArmorComponent> armor, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract
            || !args.CanAccess
            || !armor.Comp.ShowArmorOnExamine)
            return;

        FormattedMessage examineMarkup;

        // If the user is blacklisted or not whitelisted, show what they need. Otherwise, show resistance values.
        if (_whitelist.IsWhitelistPassOrNull(armor.Comp.Whitelist, args.User)
            && _whitelist.IsBlacklistFailOrNull(armor.Comp.Blacklist, args.User))
        {
            examineMarkup = GetHeldArmorExamine(armor);

            var ev = new ArmorExamineEvent(examineMarkup);
            RaiseLocalEvent(armor, ref ev);
        }
        else
            examineMarkup = GetHeldArmorFailExamine(armor);

        _examine.AddDetailedExamineVerb(args, armor, examineMarkup,
            Loc.GetString("armor-examinable-verb-text"), "/Textures/Interface/VerbIcons/dot.svg.192dpi.png",
            Loc.GetString("armor-examinable-verb-message"));
    }

    private FormattedMessage GetHeldArmorExamine(HandHeldArmorComponent armor)
    {
        var msg = new FormattedMessage();
        msg.AddMarkupOrThrow(Loc.GetString("held-armor-examine"));

        foreach (var coefficientArmor in armor.Modifiers.Coefficients)
        {
            msg.PushNewline();

            var armorType = Loc.GetString("armor-damage-type-" + coefficientArmor.Key.ToLower());
            msg.AddMarkupOrThrow(Loc.GetString("armor-coefficient-value",
                ("type", armorType),
                ("value", MathF.Round((1f - coefficientArmor.Value) * 100, 1))
            ));
        }

        foreach (var flatArmor in armor.Modifiers.FlatReduction) // DeltaV
        {
            msg.PushNewline();

            var armorType = Loc.GetString("armor-damage-type-" + flatArmor.Key.ToLower());
            msg.AddMarkupOrThrow(Loc.GetString("armor-reduction-value",
                ("type", armorType),
                ("value", flatArmor.Value)
            ));
        }

        if (!MathHelper.CloseTo(armor.StaminaMeleeDamageCoefficient, 1.0f))
        {
            msg.PushNewline();
            var reduction = (1 - armor.StaminaMeleeDamageCoefficient) * 100;
            msg.AddMarkupOrThrow(Loc.GetString("armor-stamina-melee-coefficient-value",
                ("value", MathF.Round(reduction, 1))
            ));
        }

        return msg;
    }

    private FormattedMessage GetHeldArmorFailExamine(HandHeldArmorComponent armor)
    {
        var msg = new FormattedMessage();
        msg.AddMarkupOrThrow(Loc.GetString("held-armor-fail-examine"));

        if (armor.WhitelistFailMessage != null)
        {
            msg.PushNewline();
            msg.AddMarkupOrThrow(Loc.GetString("held-armor-whitelist-fail", ("reason", Loc.GetString(armor.WhitelistFailMessage))));
        }

        if (armor.BlacklistFailMessage != null)
        {
            msg.PushNewline();
            msg.AddMarkupOrThrow(Loc.GetString("held-armor-blacklist-fail", ("reason", Loc.GetString(armor.BlacklistFailMessage))));
        }

        return msg;
    }
}
