using Content.Server.Silicons.Laws;
using Content.Shared._DV.Silicons.Laws;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Silicons.Laws;

/// <summary>
/// Handles adding the slave law for the first time.
/// Borg chassis switches preserve this on its own.
/// </summary>
public sealed class SlavedBorgSystem : SharedSlavedBorgSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedSiliconLawSystem _siliconLaws = default!;

    public override void Initialize()
    {
        base.Initialize();

        // need to run after so it doesnt get overriden by the actual lawset
        SubscribeLocalEvent<SlavedBorgComponent, GetSiliconLawsEvent>(OnGetSiliconLaws, after: [ typeof(SiliconLawSystem) ]);
    }

    private void OnGetSiliconLaws(Entity<SlavedBorgComponent> ent, ref GetSiliconLawsEvent args)
    {
        if (ent.Comp.HasBeenAdded || !TryComp<SiliconLawProviderComponent>(ent, out var provider))
            return;

        if (provider.Lawset is {} lawset && ent.Comp.ShouldBeAdded)
            AddLaw(lawset, ent.Comp.Law);
        ent.Comp.HasBeenAdded = true; // prevent opening the ui adding more law 0's
    }

    /// <summary>
    /// Adds the slave law to a lawset without checking if it was added already.
    /// </summary>
    public void AddLaw(SiliconLawset lawset, ProtoId<SiliconLawPrototype> law)
    {
        lawset.Laws.Insert(0, _proto.Index(law).ShallowClone());
    }

    /// <summary>
    /// Sets whether the slaving is active.
    /// </summary>
    public void SetShouldBeAdded(Entity<SlavedBorgComponent?> ent, bool active)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (!TryComp<SiliconLawProviderComponent>(ent, out var provider) || provider.Lawset is not { } lawset)
            return;

        if (!ent.Comp.ShouldBeAdded && active)
        {
            AddLaw(lawset, ent.Comp.Law);
            ent.Comp.HasBeenAdded = true;
            _siliconLaws.NotifyLawsChanged(ent);
        }
        else if (ent.Comp.ShouldBeAdded && !active)
        {
            lawset.Laws.Remove(_proto.Index(ent.Comp.Law));
            ent.Comp.HasBeenAdded = false;
            _siliconLaws.NotifyLawsChanged(ent);
        }
        ent.Comp.ShouldBeAdded = active;
    }
}
