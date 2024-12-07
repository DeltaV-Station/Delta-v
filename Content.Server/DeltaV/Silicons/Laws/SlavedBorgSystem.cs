using Content.Server.Silicons.Laws;
using Content.Shared.DeltaV.Silicons.Laws;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.DeltaV.Silicons.Laws;

/// <summary>
/// Handles adding the slave law for the first time.
/// Borg chassis switches preserve this on its own.
/// </summary>
public sealed class SlavedBorgSystem : SharedSlavedBorgSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        // need to run after so it doesnt get overriden by the actual lawset
        SubscribeLocalEvent<SlavedBorgComponent, GetSiliconLawsEvent>(OnGetSiliconLaws, after: [ typeof(SiliconLawSystem) ]);
    }

    private void OnGetSiliconLaws(Entity<SlavedBorgComponent> ent, ref GetSiliconLawsEvent args)
    {
        if (ent.Comp.Added || !TryComp<SiliconLawProviderComponent>(ent, out var provider))
            return;

        if (provider.Lawset is {} lawset)
            AddLaw(lawset, ent.Comp.Law);
        ent.Comp.Added = true; // prevent opening the ui adding more law 0's
    }

    /// <summary>
    /// Adds the slave law to a lawset without checking if it was added already.
    /// </summary>
    public void AddLaw(SiliconLawset lawset, ProtoId<SiliconLawPrototype> law)
    {
        lawset.Laws.Insert(0, _proto.Index(law));
    }
}
