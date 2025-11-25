namespace Content.Shared._DV.Psionics.Systems;

/// <summary>
/// The system to deal with all psionics. Each part of the System is in a subsystem.
/// </summary>
public sealed partial class PsionicSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        InitializeItems();
        InitializeAbilities();
    }
}
