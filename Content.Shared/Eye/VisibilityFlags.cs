using Robust.Shared.Serialization;

namespace Content.Shared.Eye
{
    [Flags]
    [FlagsFor(typeof(VisibilityMaskLayer))]
    public enum VisibilityFlags : int
    {
        None   = 0,
        Normal = 1 << 0,
        Ghost  = 1 << 1,
        Subfloor = 1 << 3, // DeltaV - 4 is occupied by PsionicInvisibility and changing that massively fucks up stuff
        // Begin DeltaV Additions
        PsionicInvisibility = 1 << 2,
        TelegnosticProjection = PsionicInvisibility | Normal
        // End DeltaV Additions
    }
}
