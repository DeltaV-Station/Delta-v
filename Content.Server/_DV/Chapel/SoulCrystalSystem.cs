using Content.Shared.Mind;
using Content.Shared._DV.Chapel;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Chapel;

public sealed class SoulCrystalSystem : SharedSoulCrystalSystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;

    public override void Initialize()
    {
        base.Initialize();

        // TODO: mind added event
        // TODO: mind removed event
    }

    /// <summary>
    /// Create a soul crystal and transfer a mind into it at the given location.
    /// </summary>
    public void SealSoul(EntProtoId sealProto, EntityUid mindID, EntityUid target, EntityCoordinates coords)
    {
        var sealingEnt = Spawn(sealProto, coords);
        _mind.TransferTo(mindID, sealingEnt);

        var targetName = MetaData(target).EntityName;

        if (TryComp<SoulCrystalComponent>(sealingEnt, out var crystalComp))
            crystalComp.TrueName = targetName;

        _meta.SetEntityName(sealingEnt, Loc.GetString("soul-crystal-entity-name", ("sealed", targetName)));
        _meta.SetEntityDescription(sealingEnt, Loc.GetString("soul-crystal-entity-desc", ("sealed", targetName)));
    }
}
