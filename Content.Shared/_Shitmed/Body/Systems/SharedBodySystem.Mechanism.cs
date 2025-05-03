using Content.Shared._Shitmed.Body.Components;
using Content.Shared._Shitmed.Body.Events;

namespace Content.Shared.Body.Systems;

public partial class SharedBodySystem
{
    private EntityQuery<BodyMechanismComponent> _mechanismQuery;

    private void InitializeMechanism()
    {
        _mechanismQuery = GetEntityQuery<BodyMechanismComponent>();
    }

    public bool TryEnableMechanism(Entity<BodyMechanismComponent?> ent)
    {
        if (!_mechanismQuery.Resolve(ent, ref ent.Comp, false) || ent.Comp.Enabled || ent.Comp.Body is not {} body)
            return false;

        var attemptEv = new MechanismEnableAttemptEvent(body);
        RaiseLocalEvent(ent, ref attemptEv);
        if (attemptEv.Cancelled)
            return false;

        ent.Comp.Enabled = true;
        Dirty(ent, ent.Comp);

        var ev = new MechanismEnabledEvent(body);
        RaiseLocalEvent(ent, ref ev);
        return true;
    }

    public bool TryDisableMechanism(Entity<BodyMechanismComponent?> ent)
    {
        if (!_mechanismQuery.Resolve(ent, ref ent.Comp, false) || !ent.Comp.Enabled || ent.Comp.Body is not {} body)
            return false;

        var attemptEv = new MechanismDisableAttemptEvent(body);
        RaiseLocalEvent(ent, ref attemptEv);
        if (attemptEv.Cancelled)
            return false;

        ent.Comp.Enabled = false;
        Dirty(ent, ent.Comp);

        var ev = new MechanismDisabledEvent(body);
        RaiseLocalEvent(ent, ref ev);
        return true;
    }

    /// <summary>
    /// Set the body for a mechanism, enabling it if possible.
    /// </summary>
    public void SetBody(Entity<BodyMechanismComponent?> ent, EntityUid? body)
    {
        if (!_mechanismQuery.Resolve(ent, ref ent.Comp, false) || body == ent.Comp.Body)
            return;

        // have to set it to null first, can't just "teleport" it into a different body
        if (body != null && ent.Comp.Body != null)
            return;

        // disable it before removing
        if (ent.Comp.Body != null)
            TryDisableMechanism(ent);

        var old = ent.Comp.Body;
        ent.Comp.Body = body;
        Dirty(ent, ent.Comp);

        if (old is {} oldBody)
        {
            var ev = new MechanismRemovedEvent(oldBody);
            RaiseLocalEvent(ent, ref ev);
        }
        if (body is {} newBody)
        {
            var ev = new MechanismAddedEvent(newBody);
            RaiseLocalEvent(ent, ref ev);
        }

        // enalbe it after adding
        if (ent.Comp.Body != null)
            TryEnableMechanism(ent);
    }

    /// <summary>
    /// Get the body set for a mechanism.
    /// </summary>
    public EntityUid? GetBody(EntityUid uid)
    {
        return _mechanismQuery.CompOrNull(uid)?.Body;
    }

    /// <summary>
    /// Returns true if a mechanism is enabled.
    /// </summary>
    public bool IsEnabled(EntityUid uid)
    {
        return _mechanismQuery.CompOrNull(uid)?.Enabled ?? false;
    }

    /// <summary>
    /// Repeat enable/disable events for a mechanism.
    /// </summary>
    public void UpdateMechanism(EntityUid uid, EntityUid body)
    {
        if (IsEnabled(uid))
        {
            var ev = new MechanismEnabledEvent(body);
            RaiseLocalEvent(uid, ref ev);
        }
        else
        {
            var ev = new MechanismDisabledEvent(body);
            RaiseLocalEvent(uid, ref ev);
        }
    }
}
