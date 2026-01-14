using Content.Shared.Mind;
using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Tips;

/// <summary>
/// Shared tip system that provides the public API for showing tips to players.
/// </summary>
public abstract class SharedTipSystem : EntitySystem
{
    [Dependency] protected new readonly IPrototypeManager Prototype = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] protected readonly SharedMindSystem Mind = default!;

    /// <summary>
    /// Shows a tip to a player.
    /// </summary>
    /// <param name="uid">EntityUid of the player to show the tip to.</param>
    /// <param name="tipId"><see cref="TipPrototype"/> to use, wrapped in a <see cref="ProtoId{T}"/></param>
    /// <param name="ignoreCvar">Whether to ignore the DisableTips CCvar on the client.</param>
    /// <param name="sound">Optional SoundSpecifier, will play the prototype's default sound if null.</param>
    [PublicAPI]
    public void ShowTip(EntityUid uid, ProtoId<TipPrototype> tipId, bool ignoreCvar = false, SoundSpecifier? sound = null)
    {
        if (!Prototype.TryIndex(tipId, out var tipProto))
        {
            Log.Warning($"Attempted to show non-existent tip: {tipId}");
            return;
        }

        if (!Mind.TryGetMind(uid, out _, out var mind) || !_player.TryGetSessionById(mind.UserId, out var session))
            return;

        ShowTip(session, tipProto, ignoreCvar, sound);
    }

    /// <summary>
    /// Shows a tip to a player.
    /// </summary>
    /// <param name="session"><see cref="ICommonSession"/> of the player to show the tip to.</param>
    /// <param name="tipId"><see cref="TipPrototype"/> to use, wrapped in a <see cref="ProtoId{T}"/></param>
    /// <param name="ignoreCvar">Whether to ignore the DisableTips CCvar on the client.</param>
    /// <param name="sound">Optional SoundSpecifier, will play the prototype's default sound if null.</param>
    [PublicAPI]
    public void ShowTip(ICommonSession session, ProtoId<TipPrototype> tipId, bool ignoreCvar = false, SoundSpecifier? sound = null)
    {
        if (!Prototype.TryIndex(tipId, out var tipProto))
        {
            Log.Warning($"Attempted to show non-existent tip: {tipId}");
            return;
        }

        ShowTip(session, tipProto, ignoreCvar, sound);
    }

    /// <summary>
    /// Shows a tip to a player.
    /// </summary>
    /// <param name="session"><see cref="ICommonSession"/> of the player to show the tip to.</param>
    /// <param name="tip"><see cref="TipPrototype"/> to use.</param>
    /// <param name="ignoreCvar">Whether to ignore the DisableTips CCvar on the client.</param>
    /// <param name="sound">Optional SoundSpecifier, will play the prototype's default sound if null.</param>
    [PublicAPI]
    public void ShowTip(ICommonSession session, TipPrototype tip, bool ignoreCvar = false, SoundSpecifier? sound = null)
    {
        RaiseNetworkEvent(new ShowTipEvent(tip.ID, sound ?? tip.Sound, ignoreCvar), session);
    }
}
