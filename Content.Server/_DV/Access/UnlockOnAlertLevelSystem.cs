using Content.Server.AlertLevel;
using Content.Shared._DV.Access;
using Content.Shared.Lock;

namespace Content.Server._DV.Access;

/// <summary>
///     When alert level is changed, checks if any entities with an UnlockOnAlertLevelComponent need to be opened.
/// </summary>

public sealed class UnlockOnAlertLevelSystem : SharedUnlockOnAlertLevelSystem
{
    [Dependency] private readonly LockSystem _lock = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlertLevelChangedEvent>(OnAlertLevelChanged);
    }

    private void OnAlertLevelChanged(AlertLevelChangedEvent args)
    {
        var query = EntityQueryEnumerator<UnlockOnAlertLevelComponent, LockComponent>();
        while (query.MoveNext(out var uid, out var unlockComp, out var lockComp))
        {
            if (lockComp.Locked == false) continue;
            foreach (var level in unlockComp.AlertLevels)
            {
                if (level == args.AlertLevel)
                {
                    _lock.Unlock(uid, null, lockComp);
                    break;
                }
            }
        }
    }
}
