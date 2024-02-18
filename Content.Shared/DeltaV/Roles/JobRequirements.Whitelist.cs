/*
 * Delta-V - This file is licensed under AGPLv3
 * Copyright (c) 2024 Delta-V Contributors
 * See AGPLv3.txt for details.
*/

using JetBrains.Annotations;
using Robust.Shared.Serialization;

namespace Content.Shared.Roles
{
    [UsedImplicitly]
    [Serializable, NetSerializable]
    public sealed partial class WhitelistRequirement : JobRequirement
    {
    }
}
