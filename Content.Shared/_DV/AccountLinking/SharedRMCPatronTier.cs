using Robust.Shared.Serialization;

namespace Content.Shared._DV.AccountLinking;

[Serializable, NetSerializable]
public sealed record SharedDeltaVPatronTier(
    bool ShowOnCredits,
    string Tier
);
