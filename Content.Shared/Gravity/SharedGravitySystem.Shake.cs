using Content.Shared._ES.Camera; // ES
using Content.Shared.GameTicking;
using Robust.Shared.Player; // ES
using Robust.Shared.Timing;

namespace Content.Shared.Gravity;

public abstract partial class SharedGravitySystem
{
    // ES START
    [Dependency] private readonly SharedGameTicker _ticker = default!;
    [Dependency] private readonly SharedESScreenshakeSystem _shake = default!;
    // ES END

    protected const float GravityKick = 100.0f;
    protected const float ShakeCooldown = 0.2f;

    private void UpdateShake()
    {
        var curTime = Timing.CurTime;
        var gravityQuery = GetEntityQuery<GravityComponent>();
        var query = EntityQueryEnumerator<GravityShakeComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.NextShake <= curTime)
            {
                if (comp.ShakeTimes == 0 || !gravityQuery.TryGetComponent(uid, out var gravity))
                {
                    RemCompDeferred<GravityShakeComponent>(uid);
                    continue;
                }

                ShakeGrid(uid, gravity);
                comp.ShakeTimes--;
                comp.NextShake += TimeSpan.FromSeconds(ShakeCooldown);
                Dirty(uid, comp);
            }
        }
    }

    public void StartGridShake(EntityUid uid, GravityComponent? gravity = null)
    {
        if (Terminating(uid))
            return;

        if (!Resolve(uid, ref gravity, false))
            return;

        // ES START
        // do not shake grid if the round just started
        // i did not want to have to think this logic through more. this is the simplest solution i could think of
        if (Timing.CurTime - _ticker.RoundStartTimeSpan < TimeSpan.FromSeconds(30))
            return;

        // ES SCREENSHAKE LOGIC
        // instead of poopass camera kick
        var translation = new ESScreenshakeParameters() { Trauma = 0.8f, DecayRate = 0.04f, Frequency = 0.015f };
        var filter = Filter.BroadcastGrid(uid);
        _shake.Screenshake(filter, translation, null);

        return;
        // ES END
        if (!TryComp<GravityShakeComponent>(uid, out var shake))
        {
            shake = AddComp<GravityShakeComponent>(uid);
            shake.NextShake = Timing.CurTime;
        }

        shake.ShakeTimes = 10;
        Dirty(uid, shake);
    }

    protected virtual void ShakeGrid(EntityUid uid, GravityComponent? comp = null) {}
}
