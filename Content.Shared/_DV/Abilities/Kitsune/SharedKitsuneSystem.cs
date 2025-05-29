using Content.Shared._DV.Humanoid;
using Content.Shared.Actions;
using Content.Shared.Charges.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.Popups;

namespace Content.Shared._DV.Abilities.Kitsune;

public abstract class SharedKitsuneSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] protected readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly SharedPointLightSystem _light = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KitsuneComponent, CreateFoxfireActionEvent>(OnCreateFoxfire);
        SubscribeLocalEvent<FoxfireComponent, ComponentShutdown>(OnFoxfireShutdown);
        SubscribeLocalEvent<KitsuneComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<KitsuneComponent, AppearanceLoadedEvent>(OnProfileLoadFinished);
    }

    private void OnProfileLoadFinished(Entity<KitsuneComponent> ent, ref AppearanceLoadedEvent args)
    {
        // Eye color is stored on component to be used for fox fire/fox form color.
        if (TryComp<HumanoidAppearanceComponent>(ent, out var humanComp))
        {
            ent.Comp.Color = humanComp.EyeColor;

            var lightColor = ent.Comp.Color.Value;
            var max = MathF.Max(lightColor.R, MathF.Max(lightColor.G, lightColor.B));
            // Don't let it divide by 0
            if (max == 0)
            {
                lightColor = new Color(1, 1, 1, lightColor.A);
            }
            else
            {
                var factor = 1 / max;
                lightColor.R *= factor;
                lightColor.G *= factor;
                lightColor.B *= factor;
            }
            ent.Comp.ColorLight = lightColor;
        }
    }

    private void OnMapInit(Entity<KitsuneComponent> ent, ref MapInitEvent args)
    {
        // Kitsune Fox form should not have action to transform into fox form.
        if (!HasComp<KitsuneFoxComponent>(ent))
            _actions.AddAction(ent, ref ent.Comp.KitsuneActionEntity, ent.Comp.KitsuneAction);
        ent.Comp.FoxfireAction = _actions.AddAction(ent, ent.Comp.FoxfireActionId);
    }

    private void OnCreateFoxfire(Entity<KitsuneComponent> ent, ref CreateFoxfireActionEvent args)
    {
        // Kitsune fox can make fox fires from their mouth otherwise they need hands.
        if ((!TryComp<HandsComponent>(ent, out var hands) || hands.Count < 1) && !HasComp<KitsuneFoxComponent>(ent))
        {
            _popup.PopupEntity(Loc.GetString("fox-no-hands"), ent, ent);
            return;
        }

        args.Handled = true;

        // This caps the amount of fox fire summons at a time to the charge count, cycling the oldest fire when exceeded.
        if (ent.Comp.FoxfireAction is not {} action || _charges.IsEmpty(action))
        {
            var existing = ent.Comp.ActiveFoxFires[0];
            ent.Comp.ActiveFoxFires.RemoveAt(0);
            ent.Comp.ActiveFoxFires.Add(existing);
            Dirty(ent);
            _transform.SetCoordinates(existing, Transform(ent).Coordinates);
            return;
        }

        var fireEnt = Spawn(ent.Comp.FoxfirePrototype, Transform(ent).Coordinates);
        var fireComp = EnsureComp<FoxfireComponent>(fireEnt);
        fireComp.Kitsune = ent;
        ent.Comp.ActiveFoxFires.Add(fireEnt);
        Dirty(fireEnt, fireComp);
        Dirty(ent);

        _light.SetColor(fireEnt, ent.Comp.ColorLight ?? Color.Purple);
    }

    private void OnFoxfireShutdown(Entity<FoxfireComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Kitsune is not { } kitsune || !TryComp<KitsuneComponent>(kitsune, out var kitsuneComp))
            return;

        // Stop tracking the removed fox fire
        kitsuneComp.ActiveFoxFires.Remove(ent);
        Dirty(kitsune, kitsuneComp);

        // Refund the fox fire charge
        if (kitsuneComp.FoxfireAction is {} action)
            _charges.AddCharges(action, 1);
    }
}

public sealed partial class MorphIntoKitsune : InstantActionEvent;
