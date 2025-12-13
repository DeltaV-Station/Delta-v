using Content.Server.Mobs;
using Content.Shared._DV.Traits.Assorted;

namespace Content.Server._DV.Traits.Assorted;

public sealed class RedshirtSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RedshirtComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<RedshirtComponent> ent, ref MapInitEvent args)
    {
        if (TryComp<DeathgaspComponent>(ent, out var deathgasp))
            deathgasp.NeedsCritical = false;
    }
}
