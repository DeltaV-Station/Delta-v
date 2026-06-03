// SPDX-FileCopyrightText: 2026 Janet Blackquill <uhhadd@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared._ST.Interaction;

/// <summary>
/// Data for interaction particles
/// </summary>
[Serializable, NetSerializable]
public sealed class StellarInteractionParticleEvent(NetEntity performer, NetEntity? used, NetEntity target, bool isClientEvent) : EntityEventArgs
{
    public NetEntity Performer = performer;

    public NetEntity? Used = used;

    public NetEntity Target = target;

    /// <summary>
    /// Workaround for event subscription not working w/ the session overload
    /// </summary>
    public bool IsClientEvent = isClientEvent;
}
