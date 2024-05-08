namespace Content.Server.SimpleStation14.Clothing
{
    [RegisterComponent]
    public sealed partial class DeleteOnEquipComponent : Component
    {
        [DataField("onEquip")]
        public bool OnEquip = true;
    }
}