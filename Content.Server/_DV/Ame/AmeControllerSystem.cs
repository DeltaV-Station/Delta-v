using Content.Server.Ame.Components;
using Content.Shared.Ame.Components;
using Content.Server.Radio.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Server.Ame.EntitySystems;

public sealed partial class AmeControllerSystem
{
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private void AlertLowFuel(EntityUid uid, AmeControllerComponent controller, AmeFuelContainerComponent fuelJar)
    {
        if (fuelJar.FuelAmount > controller.FuelAlertLevel)
        {
            controller.FuelAlertCountdown = 0; // In case of refueling, this sets the countdown to immidiately trigger on next alert.
            return;
        }

        controller.FuelAlertCountdown -= controller.InjectionAmount;

        if (controller.FuelAlertCountdown > 0)
            return;

        controller.FuelAlertCountdown = controller.FuelAlertLevel / controller.FuelAlertFrequency;

        var fuelRatio = fuelJar.FuelAmount / (float)fuelJar.FuelCapacity;
        _radio.SendRadioMessage(uid,
            Loc.GetString("ame-controller-component-low-fuel-warning",
                ("percentage", Math.Round(fuelRatio * 100f))),
            _proto.Index(controller.AlertChannel), uid);
    }
}
