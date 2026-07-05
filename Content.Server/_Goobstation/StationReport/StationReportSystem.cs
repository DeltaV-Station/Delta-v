using Content.Server.GameTicking;
using Content.Shared._Goobstation.StationReport;
using Content.Shared.Paper;
using Robust.Shared.GameObjects;
using System.Text;

namespace Content.Server._Goobstation.StationReportSystem;

public sealed class StationReportSystem : EntitySystem
{

    //this is shitcode?

    public override void Initialize()
    {
        //subscribes to the endroundevent
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent args)
    {
        //locates the first entity with StationReportComponent and combines them
        var stationReports = new StringBuilder();
        var reportNumber = 1;

        var query = EntityQueryEnumerator<StationReportComponent>();
        while (query.MoveNext(out var uid, out _)) // finds every entity with StationReportComponent
        {
            if (!TryComp<PaperComponent>(uid, out var paper))
                continue;

            if (string.IsNullOrWhiteSpace(paper.Content))
                continue;

            stationReports.AppendLine($"[bold]Station Report #{reportNumber}[/bold]");
            stationReports.AppendLine(paper.Content);
            stationReports.AppendLine();

            reportNumber++;
        }
        var stationReportText = stationReports.Length > 0
            ? stationReports.ToString()
            : null;
        BroadcastStationReport(stationReportText);
    }

    //sends a networkevent to tell the client to update the stationreporttext when recived
    public void BroadcastStationReport(string? stationReportText)
    {
        RaiseNetworkEvent(new StationReportEvent(stationReportText));//to send to client
        RaiseLocalEvent(new StationReportEvent(stationReportText));//to send to discord intergration
    }
}
