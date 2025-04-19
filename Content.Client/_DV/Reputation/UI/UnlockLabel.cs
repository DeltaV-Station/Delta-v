using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Client._DV.Reputation.UI;

/// <summary>
/// Label that updates for an unlock timer automatically.
/// </summary>
public sealed class UnlockLabel : Label
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public event Action? OnUnlock;

    private TimeSpan? _nextUnlock;
    private TimeSpan _nextUpdate;

    public TimeSpan? NextUnlock
    {
        get
        {
            return _nextUnlock;
        }
        set
        {
            _nextUnlock = value;
            _nextUpdate = _timing.CurTime;
            UpdateUnlock();
        }
    }

    public bool IsLocked => Visible;

    public UnlockLabel()
    {
        IoCManager.InjectDependencies(this);

        Visible = false;
    }

    private void UpdateUnlock()
    {
        if (_nextUnlock is not {} next)
            return;

        var now = _timing.CurTime;
        if (now >= next)
        {
            // unlocked now
            _nextUnlock = null;
            Visible = false;
            OnUnlock?.Invoke();
            return;
        }

        Visible = true;
        var remaining = next - now;
        var time = $"{remaining.Minutes:00}:{remaining.Seconds:00}";
        Text = Loc.GetString("contract-next-unlock", ("time", time));
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        var now = _timing.CurTime;
        if (now < _nextUpdate)
            return;

        _nextUpdate = now + TimeSpan.FromSeconds(1);
        UpdateUnlock();
    }
}
