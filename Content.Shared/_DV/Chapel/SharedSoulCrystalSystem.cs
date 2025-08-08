using Content.Shared.Examine;

namespace Content.Shared._DV.Chapel;

public abstract class SharedSoulCrystalSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SoulCrystalComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<SoulCrystalComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("soul-crystal-examine"));
    }

}
