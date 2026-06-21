namespace Content.Server._DV.Traits.Assorted;

[RegisterComponent, Access(typeof(SlouchySystem))]
public sealed partial class SlouchyComponent : Component
{
    //tells how many stamina is drained everytime the character emotes
    [DataField]
    public float EmoteStaminaDrain = 5f;

    //tells how many stamina is drained after an action is completed (e.g. taking someone's hat off)
    [DataField]
    public float DoAfterStaminaDrain = 11f;

    //tells how many stamina is drained after speaking
    [DataField]
    public float SpeakStaminaDrain = 1f;

    //tells how many stamina is drained when attacking with your melee (hands included)
    [DataField]
    public float MeleeStaminaDrain = 8f;

    //tells howm manty stamina is drained when picking something up (e.g. items and other stuff)
    [DataField]
    public float PickupStaminaDrain = 3f;

    //tells how many stamina is drained when dropping something (e.g. refer above)
    [DataField]
    public float DropStaminaDrain = 2f;

    //tells how many stamina is drained when using an item in hand
    [DataField]
    public float UseInHandStaminaDrain = 4f;

    //tells how many stamina is drained when interacting with something (e.g. lockers, computers, etc... list goes on lol)
    [DataField]
    public float InteractStaminaDrain = 5f;
}
