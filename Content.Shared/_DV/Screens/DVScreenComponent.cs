using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Screens;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
[Access(typeof(DVSharedScreenSystem))]
public sealed partial class DVScreenComponent : Component
{
    [DataField, AutoNetworkedField]
    public string? AlertLevel; // I don't like this but uhhh the prototype isn't client-accessible

    [DataField, AutoNetworkedField]
    public bool ShowAlertBorder;

    [DataField, AutoNetworkedField]
    public DVScreenContent Content = DVScreenContent.Text;

    #region Text Screens

    [DataField, AutoNetworkedField]
    public string Line1;

    [DataField, AutoNetworkedField]
    public string Line2;

    #endregion

    #region ETA Screens

    [DataField, AutoNetworkedField]
    public bool ScreenIsAtDestination;

    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan DestinationTime;

    #endregion
}


[Serializable, NetSerializable]
public enum DVScreenVisuals : byte
{
    AlertLevel,
    ShowAlertBorder,
    Content,
    Line1,
    Line2,
    ScreenIsAtDestination,
    TargetTime,
}

[Serializable, NetSerializable]
public enum DVScreenContent : byte
{
    Text,
    CurrentTime,
    EstimatedTimeOfArrival,
}
