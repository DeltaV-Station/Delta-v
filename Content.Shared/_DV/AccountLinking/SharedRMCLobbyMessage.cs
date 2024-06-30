using Robust.Shared.Serialization;

namespace Content.Shared._DV.AccountLinking;

[Serializable, NetSerializable]
public sealed record SharedRMCLobbyMessage(string Message)
{
    public const int CharacterLimit = 40;
}
