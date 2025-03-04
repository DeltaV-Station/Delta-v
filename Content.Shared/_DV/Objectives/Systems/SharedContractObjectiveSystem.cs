namespace Content.Shared._DV.Objectives.Systems;

public abstract class SharedContractObjectiveSystem : EntitySystem
{
    public virtual string ContractName(EntityUid objective)
    {
        return Name(objective);
    }
}
