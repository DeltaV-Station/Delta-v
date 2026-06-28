using System.Linq;
using System.Globalization;
using Content.Shared.Dataset;
using Content.Shared.Humanoid;
using Content.Shared.Random.Helpers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._DV.IonLaws;

public sealed class IonLawSystem : EntitySystem
{
    [Dependency] private readonly ILocalizationManager _localization = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    private const string Culture = "en-US";

    public override void Initialize()
    {
        base.Initialize();

        var culture = new CultureInfo(Culture);
        _localization.AddFunction(culture, "PICK", FormatPick);
        _localization.AddFunction(culture, "PICK-ENTITY", FormatPickEntity);
    }

    private ILocValue FormatPick(LocArgs args)
    {
        var rand = ((LocValueRandom)args.Args[0]).Value;
        return rand.Pick(args.Args.Skip(1).ToList());
    }

    private ILocValue FormatPickEntity(LocArgs args)
    {
        var rand = ((LocValueRandom)args.Args[0]).Value;
        var players = ((LocValueEntityList)args.Args[1]).Value;
        if (players.Count == 0)
        {
            return new LocValueString(Loc.GetString("dv-ion-law-target-fallback", ("random", new LocValueRandom(rand))));
        }
        return new LocValueEntity(rand.PickAndTake(players));
    }

    private static readonly ProtoId<LocalizedDatasetPrototype> IonLawTemplates = "IonLawTemplates";

    public string GenerateLaw(IRobustRandom random)
    {
        var template = random.PickId(_proto.Index(IonLawTemplates));
        var players = _player.Sessions
            .Where(x => HasComp<HumanoidAppearanceComponent>(x.AttachedEntity))
            .Select(x => x.AttachedEntity!.Value)
            .ToList();

        return Loc.GetString(template, ("random", new LocValueRandom(random)), ("players", new LocValueEntityList(players)));
    }
}

public sealed record LocValueRandom(IRobustRandom Value) : LocValue<IRobustRandom>(Value)
{
    public override string Format(LocContext ctx)
    {
        throw new InvalidOperationException("Attempted to format a IRobustRandom");
    }
}

public sealed record LocValueEntityList(List<EntityUid> Value) : LocValue<List<EntityUid>>(Value)
{
    public override string Format(LocContext ctx)
    {
        throw new InvalidOperationException("Attempted to format a List<EntityUid>");
    }
}
