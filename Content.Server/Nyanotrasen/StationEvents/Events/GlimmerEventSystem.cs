using Content.Server.Psionics.Glimmer;
using Content.Server.StationEvents.Events;
using Content.Shared._DV.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Content.Shared.Psionics.Glimmer;

namespace Content.Server.Nyanotrasen.StationEvents.Events;

public sealed class GlimmerEventSystem : StationEventSystem<GlimmerEventComponent>
{
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;

    protected override void Ended(EntityUid uid, GlimmerEventComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);
        var glimmerBurned = RobustRandom.Next(component.GlimmerBurnLower, component.GlimmerBurnUpper);

        _glimmerSystem.Glimmer -= glimmerBurned;

        var reportEv = new GlimmerEventEndedEvent(component.SophicReport, glimmerBurned);
        RaiseLocalEvent(ref reportEv);
    }
}

