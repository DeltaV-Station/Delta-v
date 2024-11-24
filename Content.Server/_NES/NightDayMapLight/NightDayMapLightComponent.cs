namespace Content.Server._NES.NightDayMapLight
{
    [RegisterComponent]
    public sealed partial class NightDayMapLightComponent : Component
    {
        [ViewVariables]
        [DataField]
        public Color DayColor = Color.FromHex("#4d6270");

        [ViewVariables]
        [DataField]
        public Color NightColor = Color.FromHex("#010203");

        [ViewVariables]
        [DataField]
        public float DayDuration = 600;
    }
}
