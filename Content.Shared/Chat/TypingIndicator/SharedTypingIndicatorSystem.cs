using Content.Shared.Clothing.Components;
using Content.Shared.Inventory.Events;

namespace Content.Shared.Chat.TypingIndicator;

/// <summary>
///     Sync typing indicator icon between client and server.
/// </summary>
public abstract class SharedTypingIndicatorSystem : EntitySystem
{
    /// <summary>
    ///     Default ID of <see cref="TypingIndicatorPrototype"/>
    /// </summary>
    [ValidatePrototypeId<TypingIndicatorPrototype>]
    public const string InitialIndicatorId = "default";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TypingIndicatorClothingComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<TypingIndicatorClothingComponent, GotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnGotEquipped(EntityUid uid, TypingIndicatorClothingComponent component, GotEquippedEvent args)
    {
        if (!TryComp<ClothingComponent>(uid, out var clothing) ||
            !TryComp<TypingIndicatorComponent>(args.Equipee, out var indicator))
            return;

        var isCorrectSlot = clothing.Slots.HasFlag(args.SlotFlags);
        if (!isCorrectSlot) return;

        indicator.Prototype = component.Prototype;
    }

    private void OnGotUnequipped(EntityUid uid, TypingIndicatorClothingComponent component, GotUnequippedEvent args)
    {
        if (!TryComp<TypingIndicatorComponent>(args.Equipee, out var indicator))
            return;

        indicator.Prototype = SharedTypingIndicatorSystem.InitialIndicatorId;
    }

    /// <summary>
    /// Delta-V: Sets whether the typing indicator should use overrides for synths.
    /// </summary>
    public void SetUseSyntheticVariant(EntityUid uid, bool enabled)
    {
        // we need to ensurecomp here because humans and anything else that doesn't have a typingindicator out of the
        // factory will not have a typingindicator comp. (non-humans like moths have one set in the proto)
        EnsureComp<TypingIndicatorComponent>(uid, out var component);
        component.UseSyntheticVariant = enabled;
        Dirty(uid, component);
    }
}
