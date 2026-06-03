using Content.Shared._DV.KeycardAuthenticationDevice;
using Content.Shared._DV.Screens;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Communications;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedDVCommunicationsConsoleSystem))]
public sealed partial class DVCommunicationsConsoleComponent : Component
{
    [DataField]
    public LocId AnnouncementTitle = "comms-console-announcement-title-station";

    [DataField]
    public Color AnnouncementColor = Color.Gold;

    [DataField]
    public SoundSpecifier AnnouncementSound = new SoundPathSpecifier("/Audio/Announcements/announce.ogg");

    [DataField]
    public bool GlobalAnnouncements;

    [DataField]
    public bool CanAnnounce = true;

    [DataField]
    public bool CanAlertLevel = true;

    [DataField]
    public bool CanCallShuttles = true;

    [DataField]
    public bool CanConfigureScreens = true;

    [DataField]
    public bool CanKeycardAuthenticationDevice = true;

    [DataField, AutoNetworkedField]
    public TimeSpan CanAnnounceAt = TimeSpan.Zero;

    [DataField]
    public TimeSpan AnnouncementInterval = TimeSpan.FromSeconds(90f);

    [DataField]
    public TimeSpan InitialAnnouncementDelay = TimeSpan.FromSeconds(30f);

    [DataField, AutoNetworkedField]
    public string CurrentAlertLevel = string.Empty;

    [DataField, AutoNetworkedField]
    public List<DVCommunicationsConsoleAlertLevel> AlertLevels = new();

    [DataField, AutoNetworkedField]
    public TimeSpan? CanSetAlertAt = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public bool ShuttlesCallable = true;

    [DataField, AutoNetworkedField]
    public TimeSpan? ExpectedEvacuationArrival;

    [DataField, AutoNetworkedField]
    public TimeSpan? ExpectedEvacuationDuration;

    [DataField, AutoNetworkedField]
    public TimeSpan? ExpectedExfiltrationArrival;

    [DataField, AutoNetworkedField]
    public DVScreenContent LastConfiguredContent = DVScreenContent.Text;

    [DataField, AutoNetworkedField]
    public bool LastConfiguredShowBorders;

    [DataField, AutoNetworkedField]
    public string LastConfiguredLine1 = string.Empty;

    [DataField, AutoNetworkedField]
    public string LastConfiguredLine2 = string.Empty;
}

[Serializable, NetSerializable]
public readonly record struct DVCommunicationsConsoleAlertLevel(LocId AlertLevel, LocId Description, string Id, bool CanSet, Color Color);

[Serializable, NetSerializable]
public sealed class DVCommunicationsConsoleEvacuationShuttleMessage(bool call) : BoundUserInterfaceMessage
{
    public readonly bool Call = call;
}

[Serializable, NetSerializable]
public sealed class DVCommunicationsConsoleExfiltrationShuttleMessage(bool call) : BoundUserInterfaceMessage
{
    public readonly bool Call = call;
}

[Serializable, NetSerializable]
public sealed class DVCommunicationsConsoleKeycardAuthenticationDeviceMessage(DVStationKeycardAction action) : BoundUserInterfaceMessage
{
    public readonly DVStationKeycardAction Action = action;
}

[Serializable, NetSerializable]
public sealed class DVCommunicationsConsoleAnnouncementMessage(string announcement) : BoundUserInterfaceMessage
{
    public readonly string Announcement = announcement;
}

[Serializable, NetSerializable]
public sealed class DVCommunicationsConsoleAlertLevelMessage(string alertLevel) : BoundUserInterfaceMessage
{
    public readonly string AlertLevel = alertLevel;
}

[Serializable, NetSerializable]
public sealed class DVCommunicationsConsoleScreenConfigurationMessage(DVScreenContent content, bool showBorder, string line1, string line2) : BoundUserInterfaceMessage
{
    public readonly DVScreenContent Content = content;
    public readonly bool ShowBorder = showBorder;
    public readonly string Line1 = line1;
    public readonly string Line2 = line2;
}

[Serializable, NetSerializable]
public enum DVCommunicationsConsoleUi : byte
{
    Key,
}

public static class DVScreenPackets
{
    public const string Content = "dv-screen-content";
    public const string ShowBorders = "dv-screen-borders";
    public const string Text = "dv-screen-text";
}
