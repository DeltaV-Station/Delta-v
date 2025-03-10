using Content.Shared._DV.CCVars;
using Content.Shared._White.Standing;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Standing;

/// <summary>
/// Prevents shooting and makes melee weaker while you are laying down (R)
/// </summary>
public sealed class LayingDownCombatSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;

    private DamageModifierSet _meleeMod = new();
    private bool _canShoot;

    public override void Initialize()
    {
        base.Initialize();

        // subscribe to LayingDownComponent instead of StandingState so it only applies to mobs that can lie down on keypress
        SubscribeLocalEvent<LayingDownComponent, ShotAttemptedEvent>(OnShotAttempted);
        SubscribeLocalEvent<LayingDownComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);

        Subs.CVar(_cfg, DCCVars.LayingDownMeleeMod, mod =>
        {
            _meleeMod.Coefficients.Clear();
            foreach (var proto in _proto.EnumeratePrototypes<DamageTypePrototype>())
            {
                _meleeMod.Coefficients.Add(proto.ID, mod);
            }
        }, true);
        Subs.CVar(_cfg, DCCVars.LayingDownCanShoot, x => _canShoot = x, true);
    }

    private void OnShotAttempted(Entity<LayingDownComponent> ent, ref ShotAttemptedEvent args)
    {
        if (_canShoot || args.Cancelled || !_standing.IsDown(ent))
            return;

        _popup.PopupClient(Loc.GetString("gun-cant-shoot-lying-down"), args.User, args.User);
        args.Cancel();
    }

    private void OnGetMeleeDamage(Entity<LayingDownComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (!_standing.IsDown(ent))
            return;

        args.Modifiers.Add(_meleeMod);
    }
}
