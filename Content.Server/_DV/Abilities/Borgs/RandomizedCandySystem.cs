using System.Linq;
using Content.Server.Nutrition.Components;
using Content.Shared._DV.Abilities.Borgs;
using Content.Shared.Nutrition;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._DV.Abilities.Borgs;

/// <summary>
/// Gives things with a <see cref="RandomizedCandyComponent"/> a random flavor, with corresponding appearance and
/// examine text.
/// </summary>
public sealed partial class RandomizedCandySystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    /// <summary>
    /// Flavors that are masked by the candy.
    /// </summary>
    private static readonly string[] MaskedReagents = { "Sugar", "Iron" }; // sugar is obvious and iron is "metallic" :(

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RandomizedCandyComponent, MapInitEvent>(OnInit);
    }

    private void OnInit(EntityUid uid, RandomizedCandyComponent candyComp, MapInitEvent args)
    {
        // pick a random flavor
        var flavors = _prototypeManager.EnumeratePrototypes<CandyFlavorPrototype>();
        var candyFlavor = _random.Pick(flavors.ToList());

        // color the candy :3
        _appearance.SetData(uid, RandomizedCandyVisuals.Color, candyFlavor.Color);

        // flavor the candy! yummy
        var flavorProfile = EnsureComp<FlavorProfileComponent>(uid);
        flavorProfile.Flavors.Clear(); // it shouldn't be flavored but clear it anyway
        foreach (var flavorId in candyFlavor.Flavors)
        {
            flavorProfile.Flavors.Add(flavorId);
        }
        flavorProfile.IgnoreReagents.UnionWith(MaskedReagents); // otherwise the nom text gets too long

        // update the candy's metadata with fluff
        var meta = MetaData(uid);
        if (!string.IsNullOrEmpty(candyFlavor.Name))
            _metaData.SetEntityName(uid, $"{candyFlavor.Name} {meta.EntityName}", meta);
        _metaData.SetEntityDescription(uid, $"{meta.EntityDescription} {GetExamineFluff(candyFlavor.Flavors)}");
        Dirty(uid, meta);
    }

    // this technically duplicates code from FlavorProfileSystem but what we would need to call
    // is upstream code in a private method with fixed loc strings and unnecessary sorting, so i don't want to touch it
    private string GetExamineFluff(HashSet<ProtoId<FlavorPrototype>> flavorIds)
    {
        var flavors = new List<string>();
        foreach (var flavorId in flavorIds)
        {
            if (_prototypeManager.TryIndex(flavorId, out var flavor) &&
                Loc.TryGetString(flavor.FlavorDescription, out var flavorText))
            {
                flavors.Add(flavorText);
            }
        }

        return flavors.Count switch
        {
            > 1 => Loc.GetString("candy-flavor-profile-multiple",
                ("lastFlavor", flavors.Pop()),
                ("flavors", string.Join(", ", flavors))
            ),
            1 => Loc.GetString("candy-flavor-profile", ("flavor", flavors.Single())),
            _ => Loc.GetString("candy-flavor-profile-unknown")
        };
    }
}
