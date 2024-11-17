namespace Content.Server._NES.NightDayMapLight
{
    [RegisterComponent]
    public sealed partial class NightDayMapLightComponent : Component
    {
        [ViewVariables]
        [DataField]
        public Color DayColor = Color.FromHex("#646462");

        [ViewVariables]
        [DataField]
        public Color NightColor = Color.FromHex("#050607");

        [ViewVariables]
        [DataField]
        public float DayDuration = 1200;
    }
}
