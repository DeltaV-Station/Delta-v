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
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    /// <summary>
    /// Shows a tip to a player, ignoring conditions and delays.
    /// </summary>
    [PublicAPI]
    public void ShowTip(ICommonSession session, ProtoId<TipPrototype> tipId)
    {
        if (!_prototype.TryIndex(tipId, out var tipProto))
        {
            Log.Warning($"Attempted to show non-existent tip: {tipId}");
            return;
        }

        ShowTip(session, tipProto);
    }

    /// <summary>
    /// Shows a tip to a player using the prototype directly.
    /// </summary>
    [PublicAPI]
    public void ShowTip(ICommonSession session, TipPrototype tip)
    {
        RaiseNetworkEvent(new ShowTipEvent(tip.ID, tip.Sound), session);
    }

    /// <summary>
    /// Shows a tip to a player with a custom sound.
    /// </summary>
    [PublicAPI]
    public void ShowTip(ICommonSession session, ProtoId<TipPrototype> tipId, SoundSpecifier sound)
    {
        RaiseNetworkEvent(new ShowTipEvent(tipId, sound), session);
    }
}
