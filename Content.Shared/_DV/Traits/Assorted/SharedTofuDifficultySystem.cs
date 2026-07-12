namespace Content.Shared._DV.Traits.Assorted;

public abstract class SharedTofuDifficultySystem : EntitySystem
{
    public bool IsTofu(Entity<TofuDifficultyComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return false;

        return true;
    }
}
