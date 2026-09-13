using Content.Server.Salvage.Magnet;
using Content.Shared._DV.Salvage.Magnet; //DeltaV

namespace Content.Server.Salvage;

    public sealed partial class SalvageSystem
    {
        private void InitializeMagnetRelease()
        {
            SubscribeLocalEvent<SalvageMagnetComponent, MagnetReleaseEvent>(OnMagnetRelease); //DeltaV - Magnet release code
        }

        private void OnMagnetRelease(EntityUid uid, SalvageMagnetComponent component, ref MagnetReleaseEvent args)
        {
            var station = _station.GetOwningStation(uid);

            if (!TryComp(station, out SalvageMagnetDataComponent? dataComp) ||
                dataComp.EndTime == null)
            {
                return;
            }

            var curTime = _timing.CurTime;

            if (!dataComp.Announced && (dataComp.EndTime.Value - curTime).TotalSeconds > dataComp.ReleaseTime)
            {
                dataComp.EndTime = curTime + TimeSpan.FromSeconds(dataComp.ReleaseTime);
                dataComp.NextOffer = dataComp.EndTime.Value;

                UpdateMagnetUIs((station.Value, dataComp));

                var magnet = GetMagnet((station.Value, dataComp));

                if (magnet != null)
                {
                    Report(magnet.Value.Owner,
                        MagnetChannel,
                        "salvage-system-announcement-release",
                        ("timeLeft", (dataComp.EndTime.Value - curTime).Seconds));
                }
                dataComp.Announced = true;
            }
        }

    }
