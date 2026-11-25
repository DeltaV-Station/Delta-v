namespace Content.Shared._DV.Chat;


// Note for future: If you want to make this more robust to handle more types of emotes while still being able to check off audible as an option for it then it would
// probably be better to make it a struct and have audible be a flag for it and then the type of emote. This would avoid having to make two different types for audible and visual.
/// <summary>
/// Different ways of emoting. For that little extra in RP!
/// </summary>
public enum EmoteType : byte
{
    Normal, // Character emotes
    Audible, // Character screams
    Possessive, // Character's emote
    AudiblePossessive // Character's scream
}
