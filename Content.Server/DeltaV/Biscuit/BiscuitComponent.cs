/*
 * Delta-V - This file is licensed under AGPLv3
 * Copyright (c) 2024 Delta-V Contributors
 * See AGPLv3.txt for details.
*/

using Content.Shared.DeltaV.Biscuit;

namespace Content.Server.DeltaV.Biscuit;

[RegisterComponent]
public sealed partial class BiscuitComponent : SharedBiscuitComponent
{
    [DataField]
    public bool Cracked { get; set; }
}
