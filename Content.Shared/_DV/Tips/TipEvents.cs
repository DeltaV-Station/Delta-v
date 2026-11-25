using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Tips;

/// <summary>
/// Server-to-client message to display a tip popup.
/// </summary>
[Serializable, NetSerializable]
public sealed class ShowTipEvent(ProtoId<TipPrototype> tipId, SoundSpecifier? sound, bool ignoreCvar = false) : EntityEventArgs
{
    /// <summary>
    /// The prototype ID of the tip to show.
    /// </summary>
    public ProtoId<TipPrototype> TipId = tipId;

    /// <summary>
    /// The sound to play when showing the tip.
    /// Sent separately since SoundSpecifier may not serialize well across network.
    /// </summary>
    public SoundSpecifier? Sound = sound;

    /// <summary>
    /// Whether to ignore the DisableTips CCvar on the client
    /// </summary>
    public bool IgnoreCvar = ignoreCvar;
}


/// <summary>
/// Event sent from client to server when a tip popup is dismissed.
/// </summary>
[Serializable, NetSerializable]
public sealed class TipDismissedEvent(ProtoId<TipPrototype> tipId, bool dontShowAgain) : EntityEventArgs
{
    /// <summary>
    /// The prototype ID of the dismissed tip.
    /// </summary>
    public ProtoId<TipPrototype> TipId = tipId;

    /// <summary>
    /// Whether the player checked "Don't show again".
    /// </summary>
    public bool DontShowAgain = dontShowAgain;
}

/// <summary>
/// Event sent from client to server to request resetting all seen tips.
/// </summary>
[Serializable, NetSerializable]
public sealed class ResetAllSeenTipsRequest : EntityEventArgs;
