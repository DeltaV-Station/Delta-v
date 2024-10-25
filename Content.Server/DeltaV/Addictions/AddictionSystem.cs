using Content.Shared.Dataset;
using Content.Shared.DeltaV.Addictions;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Server.Player;
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

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AddictedComponent, ComponentStartup>(OnInit);
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
                UpdateSuppressed(uid, component);
                continue;
            }

            if (curTime < component.NextEffectTime)
                continue;

            DoAddictionEffect(uid);
            component.NextEffectTime = curTime + TimeSpan.FromSeconds(_random.Next(MinEffectInterval, MaxEffectInterval));
        }
    }

    // Make sure we don't get a popup on the first update
    private void OnInit(EntityUid uid, AddictedComponent component, ComponentStartup args)
    {
        var curTime = _timing.CurTime;
        component.NextEffectTime = curTime + TimeSpan.FromSeconds(_random.Next(MinEffectInterval, MaxEffectInterval));
    }

    private void UpdateSuppressed(EntityUid uid, AddictedComponent component)
    {
        var componentTime = component.LastMetabolismTime + TimeSpan.FromSeconds(10); // Ten seconds after the last metabolism cycle
        var shouldBeSupressed = (componentTime > _timing.CurTime);
        if (component.Suppressed != shouldBeSupressed)
        {
            component.Suppressed = shouldBeSupressed;
        }
        else
            return;
    }

    private string GetRandomPopup()
    {
        return Loc.GetString(_random.Pick(_prototypeManager.Index<LocalizedDatasetPrototype>("AddictionEffects").Values));
    }

    public override void TryApplyAddiction(EntityUid uid, float addictionTime, StatusEffectsComponent? status = null)
    {
        base.TryApplyAddiction(uid, addictionTime, status);
    }

    private void DoAddictionEffect(EntityUid uid)
    {
        _popup.PopupEntity(GetRandomPopup(), uid, uid);
    }

    // Called each time a reagent with the Addicted effect gets metabolized
    protected override void UpdateTime(EntityUid uid)
    {
        if (TryComp<AddictedComponent>(uid, out var component))
        {
            component.LastMetabolismTime = _timing.CurTime;
            UpdateSuppressed(uid, component);
        }
    }
}
