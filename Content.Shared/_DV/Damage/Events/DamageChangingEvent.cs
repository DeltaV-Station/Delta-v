using Content.Shared.Damage;

namespace Content.Shared._DV.Damage.Events;

/// <summary>
/// Raised before damage is done, but after resistances are calculated.
/// </summary>
public sealed class DamageChangingEvent(DamageSpecifier damage, EntityUid? origin = null) : EntityEventArgs
{
    public DamageSpecifier Damage = damage;
    public EntityUid? Origin = origin;
}
