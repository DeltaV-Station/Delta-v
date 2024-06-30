using Robust.Shared.Serialization;

namespace Content.Shared._DV.AccountLinking;

[Serializable, NetSerializable]
public sealed record SharedDeltaVPatronTier(
    bool ShowOnCredits,
    bool Figurines,
    bool LobbyMessage,
    bool RoundEndShoutout,
    string Tier
);
