using Content.Server._CD.Traits; // CD - synth trait
using Content.Server.Silicons.Laws;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Station.Components;
// CD - start synth trait
using Content.Server.Chat.Managers;
using Content.Server.Electrocution; // DV - added for synth trait
using Content.Shared.Chat;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Content.Shared._ES.Sparks; // Delta-V change - added for synth trait (where they get affected by ion storms)
using Timer = Robust.Shared.Timing.Timer; // DV - used for synth trait delayed damage
// CD - end synth trait

namespace Content.Server.StationEvents.Events;

public sealed class IonStormRule : StationEventSystem<IonStormRuleComponent>
{
    [Dependency] private readonly IonStormSystem _ionStorm = default!;
    [Dependency] private readonly IChatManager _chatManager = default!; // CD - Used for synth trait
    [Dependency] private readonly ElectrocutionSystem _electrocution = default!; // DV - used for synth trait
    [Dependency] private readonly ESSparksSystem _esSparks = default!; // DV - used for synth trait

    protected override void Started(EntityUid uid, IonStormRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation))
            return;

        // CD - Go through everyone with the SynthComponent and inform them a storm is happening.
        var synthQuery = EntityQueryEnumerator<SynthComponent>();
        while (synthQuery.MoveNext(out var ent, out var synthComp))
        {
            if (!TryComp<ActorComponent>(ent, out var actor))
                continue;

            // DV start - super duper fucked up thing for synths (deals damage, 40% chance)
            if (!RobustRandom.Prob(0.4f))
            {
                var delay = RobustRandom.Next(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(10)); // they'll never know when it'll hit them
                Timer.Spawn(delay, () =>
                {
                    _electrocution.TryDoElectrocution(ent, null, RobustRandom.Next(5, 20), TimeSpan.FromSeconds(10), true, ignoreInsulation: true);
                    _esSparks.DoSparks(ent);
                });
            }
            // DV end

            if (RobustRandom.Prob(synthComp.AlertChance))
                continue;

            var msg = Loc.GetString("station-event-ion-storm-synth");
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
            _chatManager.ChatMessageToOne(ChatChannel.Server, msg, wrappedMessage, default, false, actor.PlayerSession.Channel, colorOverride: Color.Yellow);
        }
        // CD - End of synth trait

        var query = EntityQueryEnumerator<SiliconLawBoundComponent, TransformComponent, IonStormTargetComponent>();
        while (query.MoveNext(out var ent, out var lawBound, out var xform, out var target))
        {
            // only affect law holders on the station
            if (CompOrNull<StationMemberComponent>(xform.GridUid)?.Station != chosenStation)
                continue;

            _ionStorm.IonStormTarget((ent, lawBound, target));
        }
    }
}
