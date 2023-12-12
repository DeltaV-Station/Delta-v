
namespace Content.Server.Item.PseudoItem;

    /// <summary>
    /// For entities that behave like an item under certain conditions,
    /// but not under most conditions.
    /// </summary>
[RegisterComponent]
public sealed partial class PseudoItemComponent : Component
{
    [DataField("size")]
    public int Size = 100;

    public bool Active = false;
}
