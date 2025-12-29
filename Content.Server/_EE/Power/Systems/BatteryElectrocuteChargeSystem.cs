using Content.Server.Electrocution;
using Content.Server.Popups;
using Content.Shared.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Electrocution;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Server._EE.Power.Components;

namespace Content.Server._EE.Power.Systems;

public sealed class BatteryElectrocuteChargeSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly BatterySystem _battery = default!;

    // Yes, this is absurdly small for a reason.
    public const float ElectrifiedDamagePerWatt = 0.0015f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryComponent, ElectrocutedEvent>(OnElectrocuted);
    }

    private void OnElectrocuted(EntityUid uid, BatteryComponent battery, ElectrocutedEvent args)
    {
        if (args.ShockDamage == null || args.ShockDamage <= 0)
            return;

        var charge = Math.Min(args.ShockDamage.Value * args.SiemensCoefficient
            / ElectrifiedDamagePerWatt * 2,
                battery.MaxCharge * 0.25f)
            * _random.NextFloat(0.75f, 1.25f);

        _battery.SetCharge(uid, battery.CurrentCharge + charge);

        _popup.PopupEntity(Loc.GetString("battery-electrocute-charge"), uid, uid);
    }
}
