using Content.Shared._DV.Traits.Assorted;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Traits.Assorted;

public sealed class TofuDifficultySystem : SharedTofuDifficultySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    private static readonly ProtoId<DamageModifierSetPrototype> ModifierSet = "Tofu";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TofuDifficultyComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<TofuDifficultyComponent> ent, ref MapInitEvent args)
    {
        if (TryComp<DamageableComponent>(ent, out var damageableComponent))
        {
            _damageable.SetDamageModifierSetId(ent.Owner, ModifierSet);
        }
    }
}
