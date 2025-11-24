using Content.Server._DV.Station.Components;
using Content.Server._DV.Station.Systems;
using Content.Shared._DV.Communications;

namespace Content.Server.Communications;

public sealed partial class CommunicationsConsoleSystem : EntitySystem
{
    [Dependency] private readonly StationExfiltrationSystem _stationExfiltration = default!;

    private void InitializeExfiltration()
    {
        SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleExfiltrationShuttleMessage>(OnExfiltrationMessage);
    }

    private void OnExfiltrationMessage(Entity<CommunicationsConsoleComponent> ent, ref CommunicationsConsoleExfiltrationShuttleMessage args)
    {
        if (_stationSystem.GetOwningStation(ent) is not { } station)
            return;

        if (!CanUse(args.Actor, ent))
        {
            _popupSystem.PopupEntity(Loc.GetString("comms-console-permission-denied"), ent, args.Actor);
            return;
        }

        if (args.Call)
            _stationExfiltration.Call(station);
        else
            _stationExfiltration.Recall(station);
    }
}
