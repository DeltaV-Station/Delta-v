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
        PsionicInvisibility = 1 << 2, //Nyano - Summary: adds Psionic Invisibility as a visibility layer. Currently does nothing.
        TelegnosticProjection = 5, 
    }
}
