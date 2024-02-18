/*
 * Delta-V - This file is licensed under AGPLv3
 * Copyright (c) 2024 Delta-V Contributors
 * See AGPLv3.txt for details.
*/

using Content.Server.StationEvents.Events;
using Robust.Shared.Prototypes;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(GlimmerMobRule))]
public sealed partial class GlimmerMobRuleComponent : Component
{
    [DataField(required: true)]
    public EntProtoId MobPrototype = string.Empty;
}
