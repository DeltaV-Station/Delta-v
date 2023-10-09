namespace Content.Server.Abilities.Borgs;

[RegisterComponent]
public sealed partial class FabricateCandyComponent : Component
{
    [DataField("lollipopAction")]
    public EntityUid? LollipopAction;

    [DataField("gumballAction")]
    public EntityUid? GumballAction;
}
