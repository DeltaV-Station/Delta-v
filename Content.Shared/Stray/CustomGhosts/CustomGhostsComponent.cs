using System.Numerics;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Stray.CustomGhosts;

//[Prototype("customGhost")]
[RegisterComponent, NetworkedComponent, Access(typeof(SharedCustomGhosts))]
[AutoGenerateComponentState(true)]
public sealed partial class CustomGhostsComponent : Component
{
    /// <inheritdoc/>
    //[IdDataField]
    //public string ID { get; } = default!;
    [DataField("maxRandomIndex", required: true), AutoNetworkedField]
    public int mri { get; set; } = 0;

    [DataField("ckeys")]
    public string Ckeys { get; set;} = "";

    [DataField]
    [AutoNetworkedField]
    public string currRV = "1";
    //[DataField("sprite", required: true)]
    //public ResPath CustomSpritePath { get; } = default!;

    //[DataField("alpha")]
    //public float AlphaOverride { get; } = -1;

    //[DataField("ghostName")]
    //public string GhostName = string.Empty;
//
    //[DataField("ghostDescription")]
    //public string GhostDescription = string.Empty;

    //[DataField("size")]
    //public Vector2 SizeOverride = Vector2.One;

}

//[Serializable, NetSerializable]
//public enum CustomGhostAppearance
//{
//    Sprite,
//    AlphaOverride,
//    SizeOverride
//}
