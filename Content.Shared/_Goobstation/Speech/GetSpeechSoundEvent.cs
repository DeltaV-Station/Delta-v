namespace Content.Shared._Goobstation.Speech;

[ByRefEvent]
public record struct GetSpeechSoundEvent(string? SpeechSoundProtoId = null, bool Handled = false);
