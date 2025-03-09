﻿using Content.Server.StationEvents.Events;
using Content.Shared.FixedPoint;
using Content.Shared.Nutrition.Components;
using Content.Shared._EE.Silicon.Components; // DeltaV - we have a different namespace
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Server._Goobstation.StationEvents.Metric.Components;

[RegisterComponent, Access(typeof(FoodMetricSystem))]
public sealed partial class FoodMetricComponent : Component
{
    [DataField("thirstScores", customTypeSerializer: typeof(DictionarySerializer<ThirstThreshold, FixedPoint2>))]
    public Dictionary<ThirstThreshold, FixedPoint2> ThirstScores =
        new()
        {
            { ThirstThreshold.Thirsty, 2.0f },
            { ThirstThreshold.Parched, 5.0f },
        };

    [DataField("hungerScores", customTypeSerializer: typeof(DictionarySerializer<HungerThreshold, FixedPoint2>))]
    public Dictionary<HungerThreshold, FixedPoint2> HungerScores =
        new()
        {
            { HungerThreshold.Peckish, 2.0f },
            { HungerThreshold.Starving, 5.0f },
        };

    [DataField("chargeScores", customTypeSerializer: typeof(DictionarySerializer<float, FixedPoint2>))]
    public Dictionary<float, FixedPoint2> ChargeScores =
        new()
        {
            { 0.80f, 1.0f },
            { 0.35f, 2.0f },
            { 0.10f, 5.0f },
        };

}
