namespace Content.Shared._DV.Access;

/// <summary>
///     Allows an ID card to set the access level of interacted items
/// </summary>
[RegisterComponent]
public sealed partial class ReverseAgentIDCardComponent : Component
{
    [DataField]
    public bool Overwrite;
}
