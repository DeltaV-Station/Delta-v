﻿using System.Collections.Immutable;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared._RMC14.CCVar;
using Content.Shared._RMC14.NewPlayer;
using Content.Shared.GameTicking;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Server._RMC14.NewPlayer;

public sealed class NewPlayerSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playtimeManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    private TimeSpan _newPlayerTimeTotal;
    private TimeSpan _newPlayerTimeJob;
    private TimeSpan _brandNewPlayerTimeJob;

    public override void Initialize()
    {
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

        SubscribeLocalEvent<NewPlayerLabelComponent, PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);

        ReloadPrototypes();

        Subs.CVar(_config, RMCCVars.RMCNewPlayerTimeTotalHours, v => _newPlayerTimeTotal = TimeSpan.FromHours(v), true);
        Subs.CVar(_config, RMCCVars.RMCNewPlayerTimeJobHours, v => _newPlayerTimeJob = TimeSpan.FromHours(v), true);
        Subs.CVar(_config, RMCCVars.RMCBrandNewPlayerTimeJobHours, v => _brandNewPlayerTimeJob = TimeSpan.FromHours(v), true);
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs ev)
    {
        if (ev.WasModified<PlayTimeTrackerPrototype>())
            ReloadPrototypes();
    }

    private void OnPlayerSpawnComplete(Entity<NewPlayerLabelComponent> ent, ref PlayerSpawnCompleteEvent args)
    {
        try
        {
            var times = _playtimeManager.GetPlayTimes(args.Player);
            var totalTime = TimeSpan.Zero;
            foreach (var time in times)
            {
                    totalTime += time.Value;
            }

            var jobTime = times.GetValueOrDefault(job.PlayTimeTracker);
            var newTotal = totalTime < _newPlayerTimeTotal;
            var newJob = jobTime <= _newPlayerTimeJob;
            var brandNewJob = jobTime <= _brandNewPlayerTimeJob;
            if (brandNewJob) // purple
            {
                _appearance.SetData(ent, NewPlayerLayers.Layer, NewPlayerVisuals.Four);

                var jobInfo = job.NewToJobInfo;
                var jobName = job.Name ?? string.Empty;
                if (jobInfo != null)
                {
                    var newToJobEvent = new NewToJobEvent(GetNetEntity(args.Mob), jobInfo, jobName);
                    RaiseNetworkEvent(newToJobEvent);
                }
            }
            else if (newTotal && newJob) // red
                _appearance.SetData(ent, NewPlayerLayers.Layer, NewPlayerVisuals.One);
            else if (newTotal) // yellow
                _appearance.SetData(ent, NewPlayerLayers.Layer, NewPlayerVisuals.Two);
            else if (newJob) // green
                _appearance.SetData(ent, NewPlayerLayers.Layer, NewPlayerVisuals.Three);
            else
                _appearance.RemoveData(ent, NewPlayerLayers.Layer);
        }
        catch (Exception e)
        {
            Log.Error($"Error getting new player playtime:\n{e}");
        }
    }

    private void ReloadPrototypes()
    {
        var jobs = new HashSet<ProtoId<PlayTimeTrackerPrototype>>();
        foreach (var job in _prototypes.EnumeratePrototypes<PlayTimeTrackerPrototype>())
        {
            jobs.Add(job.ID);
        }

        jobs.ToImmutableHashSet();
    }
}
