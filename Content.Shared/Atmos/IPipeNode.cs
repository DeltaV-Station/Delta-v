// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
// SPDX-License-Identifier: MIT



using Content.Shared.Atmos.Components;
using Content.Shared.Atmos;

namespace Content.Shared.Atmos;

public interface IPipeNode
{
    PipeDirection Direction { get; }
    AtmosPipeLayer Layer { get; }
}
