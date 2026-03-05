namespace Content.Shared._DV.Traits.Assorted;

public abstract class SharedRedshirtSystem : EntitySystem
{
    public bool IsRedshirt(Entity<RedshirtComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return false;

        return true;
    }
}
