using Content.Shared._DV.Surgery;
using Content.Shared.Body.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Forensics.Components;

namespace Content.Shared.Body.Systems;

/// <summary>
/// Makes bodyparts and organs filthy and dna-contaminated when a body is gibbed or butchered.
/// Adds some friction to organ harvesting -> free alien organs pipeline, you need to actually operate on the corpse.
/// </summary>
public abstract partial class SharedBodySystem
{
    [Dependency] private readonly SurgeryCleanSystem _clean = default!;

    /// <summary>
    /// Dirtiness to give giblets.
    /// </summary>
    public static readonly FixedPoint2 DirtAdded = FixedPoint2.New(100);

    private void InitializeGibDirtying()
    {
        SubscribeLocalEvent<BodyComponent, BodyGibbedEvent>(OnGibbed);
    }

    private void OnGibbed(Entity<BodyComponent> ent, ref BodyGibbedEvent args)
    {
        var dna = CompOrNull<DnaComponent>(ent)?.DNA;
        foreach (var giblet in args.Gibs)
        {
            _clean.AddDirt(giblet, DirtAdded);
            _clean.AddDna(giblet, dna);
        }
    }
}

/// <summary>
/// Raised by body system on the body entity after being gibbed.
/// </summary>
[ByRefEvent]
public record struct BodyGibbedEvent(HashSet<EntityUid> Gibs);
