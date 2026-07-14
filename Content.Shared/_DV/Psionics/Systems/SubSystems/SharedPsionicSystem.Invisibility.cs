using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Eye;

namespace Content.Shared._DV.Psionics.Systems;

public abstract partial class SharedPsionicSystem
{
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedVisibilitySystem _visibility = default!;

    private void InitializeInvisibility()
    {
        SubscribeLocalEvent<PsionicallyInvisibleComponent, MapInitEvent>(OnInvisInit);
        SubscribeLocalEvent<PsionicallyInvisibleComponent, ComponentShutdown>(OnInvisShutdown);

        SubscribeLocalEvent<PotentialPsionicComponent, MapInitEvent>(OnInit);

        SubscribeLocalEvent<PsionicallyInvisibleComponent, PsionicSuppressedEvent>(OnSuppression);
        SubscribeLocalEvent<PsionicallyInvisibleComponent, PsionicStoppedSuppressedEvent>(OnSuppressionStop);
        SubscribeLocalEvent<PotentialPsionicComponent, PsionicShieldedEvent>(OnShielded);
        SubscribeLocalEvent<PotentialPsionicComponent, PsionicStoppedShieldedEvent>(OnShieldedStop);
    }

    private void OnInvisInit(Entity<PsionicallyInvisibleComponent> invisible, ref MapInitEvent args)
    {
        if (!CanUsePsionicAbility(invisible))
            invisible.Comp.Active = false;

        SetPsionicInvisibility(invisible.Owner, invisible.Comp.Active);
    }

    private void OnInvisShutdown(Entity<PsionicallyInvisibleComponent> invisible, ref ComponentShutdown args)
    {
        SetPsionicInvisibility(invisible.Owner, false);
    }

    private void OnInit(Entity<PotentialPsionicComponent> potPsionic, ref MapInitEvent args)
    {
        SetCanSeePsionicInvisiblity(potPsionic, false);
    }

    private void OnSuppression(Entity<PsionicallyInvisibleComponent> invisible, ref PsionicSuppressedEvent args)
    {
        invisible.Comp.Active = false;
        SetPsionicInvisibility(invisible.Owner, invisible.Comp.Active);
    }

    private void OnSuppressionStop(Entity<PsionicallyInvisibleComponent> invisible, ref PsionicStoppedSuppressedEvent args)
    {
        // This event only raises when they can use psionic abilities again, so no need for a check.
        invisible.Comp.Active = true;
        SetPsionicInvisibility(invisible.Owner, invisible.Comp.Active);
    }

    private void OnShielded(Entity<PotentialPsionicComponent> potPsionic, ref PsionicShieldedEvent args)
    {
        SetCanSeePsionicInvisiblity(potPsionic, true);
    }

    private void OnShieldedStop(Entity<PotentialPsionicComponent> potPsionic, ref PsionicStoppedShieldedEvent args)
    {
        //This only gets raised when they are no longer shielded, so no need for a check.
        SetCanSeePsionicInvisiblity(potPsionic, false);
    }

    /// <summary>
    /// Enables or disables an entity to see a psionically invisible entity.
    /// </summary>
    /// <param name="potPsionic">The entity whose ability to see psionically invisible entities is being changed.</param>
    /// <param name="canSee">Whether they can see psionically invisible entities.</param>
    public void SetCanSeePsionicInvisiblity(EntityUid potPsionic, bool canSee)
    {
        if (!TryComp<EyeComponent>(potPsionic, out var eye))
            return;

        if (canSee)
            _eye.SetVisibilityMask(potPsionic, eye.VisibilityMask | (int) VisibilityFlags.PsionicInvisibility, eye);
        else
            _eye.SetVisibilityMask(potPsionic, eye.VisibilityMask & ~ (int) VisibilityFlags.PsionicInvisibility, eye);
    }

    /// <summary>
    /// Set their psionic invisibility.
    /// </summary>
    /// <param name="visible">The entity that attempts to toggle their psionic invisibility.</param>
    /// <param name="invisible">Whether they're visible or invisible.</param>
    public void SetPsionicInvisibility(Entity<VisibilityComponent?> visible, bool invisible)
    {
        if (invisible)
        {
            // Remove them from the normal layer and add them to the psionic layer.
            _visibility.AddLayer(visible, (int) VisibilityFlags.PsionicInvisibility, false);
            _visibility.RemoveLayer(visible, (int) VisibilityFlags.Normal);
        }
        else
        {
            // Remove them from the psionic layer and add them to the normal layer.
            _visibility.RemoveLayer(visible, (int) VisibilityFlags.PsionicInvisibility, false);
            _visibility.AddLayer(visible, (int) VisibilityFlags.Normal);
        }
    }
}
