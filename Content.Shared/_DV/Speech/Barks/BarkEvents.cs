using Robust.Shared.Serialization;

namespace Content.Shared._DV.Speech.Barks;

///<summary>
/// Server to client indicating a trigger for bark audio playback
/// </summary>
[Serializable, NetSerializable]
public sealed class PlayBarkEvent : EntityEventArgs
{
    public NetEntity Speaker;
    public bool IsWhisper;
    public string Message = "";
}

///<summary>
/// Local previews of a Speech Bark voice within the character editor
/// </summary>
[Serializable, NetSerializable]
public sealed class PreviewBarkEvent : EntityEventArgs;
