namespace Content.Server._DV.Uprising;

[RegisterComponent, Access(typeof(UprisingRuleSystem))]
public sealed partial class UprisingConsoleComponent : Component
{
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(5);
}
