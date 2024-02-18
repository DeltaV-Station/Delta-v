/*
 * Delta-V - This file is licensed under AGPLv3
 * Copyright (c) 2024 Delta-V Contributors
 * See AGPLv3.txt for details.
*/

using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.Harpy.Components
{
    [Serializable, NetSerializable]
    public enum HarpyVisualLayers
    {
        Singing,
    }

    [Serializable, NetSerializable]
    public enum SingingVisualLayer
    {
        True,
        False,
    }
}
