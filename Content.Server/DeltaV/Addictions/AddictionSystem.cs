using Content.Shared.Dataset;
using Content.Shared.DeltaV.Addictions;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.DeltaV.Addictions;

public sealed class AddictionSystem : SharedAddictionSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    // Define the numbers, we're not making another DeepFryerSystem.cs
    // Minimum time between popups
    private const int MinEffectInterval = 10;

    // Maximum time between popups
    private const int MaxEffectInterval = 41;

    // The time to add after the last metabolism cycle
    private const int SuppressionDuration = 10;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AddictedComponent, ComponentStartup>(OnInit);
    }

    protected override void UpdateAddictionSuppression(Entity<AddictedComponent> ent, float duration)
    {
        var curTime = _timing.CurTime;
        var newEndTime = curTime + TimeSpan.FromSeconds(duration);

        // Only update if this would extend the suppression
        if (newEndTime <= ent.Comp.SuppressionEndTime)
            return;

        ent.Comp.SuppressionEndTime = newEndTime;
        UpdateSuppressed(ent.Comp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<AddictedComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            // If it's suppressed, check if it's still supposed to be
            if (component.Suppressed)
            {
                UpdateSuppressed(component);
                continue;
            }

            if (curTime < component.NextEffectTime)
                continue;

            DoAddictionEffect(uid);
            component.NextEffectTime = curTime + TimeSpan.FromSeconds(_random.Next(MinEffectInterval, MaxEffectInterval));
        }
    }

    // Make sure we don't get a popup on the first update
    private void OnInit(Entity<AddictedComponent> ent, ref ComponentStartup args)
    {
        var curTime = _timing.CurTime;
        ent.Comp.NextEffectTime = curTime + TimeSpan.FromSeconds(_random.Next(MinEffectInterval, MaxEffectInterval));
    }

    private void UpdateSuppressed(AddictedComponent component)
    {
        component.Suppressed = (_timing.CurTime < component.SuppressionEndTime);
    }

    private string GetRandomPopup()
    {
        return Loc.GetString(_random.Pick(_prototypeManager.Index<LocalizedDatasetPrototype>("AddictionEffects").Values));
    }

    private void DoAddictionEffect(EntityUid uid)
    {
        _popup.PopupEntity(GetRandomPopup(), uid, uid);
    }

    // Called each time a reagent with the Addicted effect gets metabolized
    protected override void UpdateTime(EntityUid uid)
    {
        if (!TryComp<AddictedComponent>(uid, out var component))
            return;

        component.LastMetabolismTime = _timing.CurTime;
        component.SuppressionEndTime = _timing.CurTime + TimeSpan.FromSeconds(SuppressionDuration);
        UpdateSuppressed(component);
    }
}
