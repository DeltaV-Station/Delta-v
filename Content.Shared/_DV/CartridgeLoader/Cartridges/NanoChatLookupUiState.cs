using Robust.Shared.Serialization;

namespace Content.Shared._DV.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class NanoChatLookupUiState : BoundUserInterfaceState
{
    public readonly List<NanoChatRecipient>? Contacts;

    public NanoChatLookupUiState(List<NanoChatRecipient>? contacts)
    {
        Contacts = contacts;
    }
}
