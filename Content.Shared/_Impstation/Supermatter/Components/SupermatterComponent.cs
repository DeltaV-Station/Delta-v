using System.Collections.Frozen;
using Content.Shared._Impstation.Supermatter.Prototypes;
using Content.Shared.Atmos;
using Content.Shared.DeviceLinking;
using Content.Shared.DoAfter;
using Content.Shared.Radio;
using Content.Shared.Speech;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Impstation.Supermatter.Components;

[RegisterComponent][AutoGenerateComponentPause][NetworkedComponent][AutoGenerateComponentState(true, true)]
public sealed partial class SupermatterComponent : Component
{
    #region Base

    /// <summary>
    /// The current status of the singularity, used for alert sounds and the monitoring console
    /// </summary>
    [DataField][AutoNetworkedField]
    public SupermatterStatusType Status = SupermatterStatusType.Inactive;

    /// <summary>
    /// The supermatter's internal gas storage
    /// </summary>
    [DataField][ViewVariables(VVAccess.ReadOnly)][AutoNetworkedField]
    public GasMixture? GasStorage;

    [DataField]
    public Color LightColorNormal = Color.FromHex("#ffe000");

    [DataField]
    public Color LightColorDelam = Color.FromHex("#ff5555");

    #endregion

    #region Prototypes

    [DataField]
    public EntProtoId SliverPrototype = "SupermatterSliver";

    [DataField]
    public EntProtoId AnomalyBluespaceSpawnPrototype = "AnomalyBluespace";

    [DataField]
    public EntProtoId AnomalyGravitySpawnPrototype = "AnomalyGravity";

    [DataField]
    public EntProtoId AnomalyPyroSpawnPrototype = "AnomalyPyroclastic";

    [DataField]
    public EntProtoId CollisionResultPrototype = "Ash";

    [DataField]
    public List<ProtoId<SupermatterDelaminationPrototype>> EnabledDelaminations = new();
    
    [DataField]
    public ProtoId<SupermatterDelaminationPrototype> DefaultDelamination = "SupermatterDelaminationExplosion";
    
    #endregion

    #region Sounds

    [DataField]
    public SoundSpecifier DustSound = new SoundPathSpecifier("/Audio/_Impstation/Supermatter/supermatter.ogg");

    [DataField]
    public SoundSpecifier DistortSound = new SoundPathSpecifier("/Audio/_Impstation/Supermatter/charge.ogg");

    [DataField]
    public SoundSpecifier PullSound = new SoundPathSpecifier("/Audio/_Impstation/Supermatter/marauder.ogg");

    [DataField]
    public SoundSpecifier CalmLoopSound = new SoundPathSpecifier("/Audio/_Impstation/Supermatter/calm.ogg");

    [DataField]
    public SoundSpecifier DelamLoopSound = new SoundPathSpecifier("/Audio/_Impstation/Supermatter/delamming.ogg");

    [DataField]
    public SoundSpecifier CalmAccent = new SoundCollectionSpecifier("SupermatterAccentNormal");

    [DataField]
    public SoundSpecifier DelamAccent = new SoundCollectionSpecifier("SupermatterAccentDelam");

    [DataField]
    public ProtoId<SpeechSoundsPrototype> StatusSilentSound = "SupermatterSilent";

    [DataField]
    public ProtoId<SpeechSoundsPrototype> StatusWarningSound = "SupermatterWarning";

    [DataField]
    public ProtoId<SpeechSoundsPrototype> StatusDangerSound = "SupermatterDanger";

    [DataField]
    public ProtoId<SpeechSoundsPrototype> StatusEmergencySound = "SupermatterEmergency";

    [DataField]
    public ProtoId<SpeechSoundsPrototype> StatusDelamSound = "SupermatterDelaminating";

    #endregion

    #region Processing

    /// <summary>
    /// The internal energy of the supermatter
    /// </summary>
    [DataField][AutoNetworkedField]
    public float Power;

    /// <summary>
    /// Takes the energy that supermatter collision generates and slowly turns it into actual power
    /// </summary>
    [DataField][AutoNetworkedField]
    public float MatterPower;
    
    /// <summary>
    /// The percentage of the gas on the supermatter's tile that is absorbed and evaluated each atmos tick. The absorbed gasses are stored in <see cref="GasStorage"/> until the next atmos tick.
    /// </summary>
    /// <remarks>Waste gasses are added to the evaluated gas mixture, and the new mixture is released after processing.</remarks>
    [DataField]
    public float GasEfficiency = 0.05f;

    /// <summary>
    /// The proportion of the absorbed gas to void. The gas is voided from the tile mixture rather than the gas storage
    /// to preserve the accuracy of the gas storage for other functions. 
    /// </summary>
    /// <remarks>
    /// In EE and Impstation, the supermatter voids gas due to an undocumented feature or bug where the absorbed gas was
    /// being removed a second time during the damage step, and was never re-added.
    /// </remarks>
    [DataField]
    public float GasVoidProportion;

    /// <summary>
    /// Uses <see cref="PowerlossDynamicScaling"/> and <see cref="GasStorage"/> to lessen the effects of our powerloss functions
    /// </summary>
    [DataField][ViewVariables(VVAccess.ReadOnly)]
    public float PowerlossInhibitor = 1;

    /// <summary>
    /// Based on CO2 percentage, this slowly moves between 0 and 1.
    /// We use it to calculate <see cref="PowerlossInhibitor"/>.
    /// </summary>
    [DataField][ViewVariables(VVAccess.ReadOnly)]
    public float PowerlossDynamicScaling;

    /// <summary>
    /// Affects the amount of damage and minimum point at which the SM takes heat damage
    /// </summary>
    [DataField][ViewVariables(VVAccess.ReadOnly)]
    public float DynamicHeatResistance = 1;

    /// <summary>
    /// More moles of gases are harder to heat than fewer, so let's scale heat damage around them
    /// </summary>
    [DataField][ViewVariables(VVAccess.ReadOnly)]
    public float MoleHeatPenaltyThreshold;

    /// <summary>
    /// The lifetime of a supermatter-spawned anomaly.
    /// </summary>
    [DataField]
    public float AnomalyLifetime = 60f;

    /// <summary>
    /// The minimum distance from the supermatter that anomalies will spawn at
    /// </summary>
    [DataField]
    public float AnomalySpawnMinRange = 5f;

    /// <summary>
    /// The maximum distance from the supermatter that anomalies will spawn at
    /// </summary>
    [DataField]
    public float AnomalySpawnMaxRange = 10f;

    /// <summary>
    /// The chance for a bluespace anomaly to spawn when power or damage is high
    /// </summary>
    [DataField]
    public float AnomalyBluespaceChance = 150f;

    /// <summary>
    /// The chance for a gravity anomaly to spawn when power or damage is high, and the severe power penalty threshold is exceeded
    /// </summary>
    [DataField]
    public float AnomalyGravityChanceSevere = 150f;

    /// <summary>
    /// The chance for a gravity anomaly to spawn when power or damage is high
    /// </summary>
    [DataField]
    public float AnomalyGravityChance = 750f;

    /// <summary>
    /// The chance for a pyroclastic anomaly to spawn when power or damage is high, and the severe power penalty threshold is exceeded
    /// </summary>
    [DataField]
    public float AnomalyPyroChanceSevere = 375f;

    /// <summary>
    /// The chance for a pyroclastic anomaly to spawn when power or damage is high, and the power penalty threshold is exceeded
    /// </summary>
    [DataField]
    public float AnomalyPyroChance = 2500f;

    /// <summary>
    /// The base number of rads produced by the crystal.
    /// </summary>
    [DataField]
    public float RadsBase = 4.0f;

    /// <summary>
    /// The multiplier used to scale the bonus rads produced by the supermatter.
    /// </summary>
    [DataField]
    public float RadsModifier = 1.0f;

    /// <summary>
    /// The waste modifier without the <see cref="GasWasteModifierMinimum"/> applied.
    /// </summary>
    [DataField][ViewVariables(VVAccess.ReadOnly)][AutoNetworkedField]
    public float GasWasteModifier;

    /// <summary>
    /// The minimum functional value of <see cref="GasWasteModifier"/>.
    /// </summary>
    [DataField]
    public float GasWasteModifierMinimum = 0.5f;

    #endregion

    #region Timing

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))][AutoPausedField][AutoNetworkedField]
    public TimeSpan? AnnounceNext;
    
    [DataField]
    public TimeSpan AnnounceInterval = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Time when the delamination will occur
    /// </summary>
    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer))][AutoPausedField][AutoNetworkedField]
    public TimeSpan? DelaminationTime;

    /// <summary>
    /// How long it takes in seconds for the supermatter to delaminate after reaching zero integrity
    /// </summary>
    [DataField][AutoNetworkedField]
    public TimeSpan DelaminationDelay = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Last time a supermatter accent sound was triggered
    /// </summary>
    [DataField][AutoNetworkedField]
    public TimeSpan AccentLastTime;

    /// <summary>
    /// Minimum time in seconds between supermatter accent sounds
    /// </summary>
    [DataField]
    public float AccentMinCooldown = 2f;

    #endregion

    #region Damage

    /// <summary>
    /// The amount of damage taken
    /// </summary>
    [DataField][AutoNetworkedField]
    public float Damage;

    /// <summary>
    /// The damage from before this cycle.
    /// Used to limit the damage we can take each cycle, and for safe alert.
    /// </summary>
    [DataField][ViewVariables(VVAccess.ReadOnly)][AutoNetworkedField]
    public float DamageArchived;

    /// <summary>
    /// The maximum amount of damage the supermatter can take in a single cycle, proportional to <see cref="DamageDelaminationThreshold"/>
    /// </summary>
    [DataField]
    public float MaximumDamagePerCycle = 0.002f;

    /// <summary>
    /// Supermatter Damage is multiplied by this value.
    /// </summary>
    [DataField]
    public float DamageMultiplier = 0.25f;

    /// <summary>
    /// Max space damage the SM will take per cycle
    /// </summary>
    [DataField]
    public float MaxSpaceExposureDamage = 2;

    /// <summary>
    /// The point at which the SM begins shooting lightning.
    /// </summary>
    [DataField]
    public float DamagePenaltyPoint = 550;

    /// <summary>
    /// The point at which we should start sending radio messages about the damage.
    /// </summary>
    [DataField]
    public float DamageWarningThreshold = 50;

    /// <summary>
    /// The point at which the SM begins showing warning signs.
    /// </summary>
    [DataField]
    public float DamageDangerThreshold = 300;

    /// <summary>
    /// The point at which we start sending station announcements about the damage.
    /// </summary>
    [DataField]
    public float DamageEmergencyThreshold = 500;
    
    /// <summary>
    /// The point at which the SM begins delaminating.
    /// </summary>
    [DataField]
    public float DamageDelaminationThreshold = 900;

    /// <summary>
    /// Whether the SM is currently in the delaminating process. 
    /// </summary>
    /// <remarks>
    /// <para>This can be false while the <see cref="Status"/> is <see cref="SupermatterStatusType.Delaminating"/>, such as when the crystal is just entering the delamination process. If you don't need the distinction, use Status instead.</para>
    /// </remarks>
    [DataField][AutoNetworkedField]
    public bool IsDelaminating;

    /// <summary>
    /// The selected delamination type to occur.
    /// </summary>
    [DataField]
    public SupermatterDelaminationPrototype? PreferredDelamination;

    #endregion

    #region Announcements

    /// <summary>
    /// Whether the current delamination process has been announced.
    /// </summary>
    [DataField]
    public bool IsDelaminationAnnounced;

    /// <summary>
    /// Whether to suppress announcements for this supermatter.
    /// </summary>
    [DataField]
    public bool SuppressAnnouncements;


    /// <summary>
    /// The radio channel for supermatter alerts
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype> Channel = "Engineering";

    /// <summary>
    /// The common radio channel for severe supermatter alerts
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype> ChannelGlobal = "Common";

    /// <summary>
    /// Used for logging if the supermatter has been powered
    /// </summary>
    [DataField]
    public bool HasBeenPowered;

    #endregion

    #region Signal Ports

    [DataField]
    public ProtoId<SourcePortPrototype> PortInactive = "SupermatterInactive";

    [DataField]
    public ProtoId<SourcePortPrototype> PortNormal = "SupermatterNormal";

    [DataField]
    public ProtoId<SourcePortPrototype> PortCaution = "SupermatterCaution";

    [DataField]
    public ProtoId<SourcePortPrototype> PortWarning = "SupermatterWarning";

    [DataField]
    public ProtoId<SourcePortPrototype> PortDanger = "SupermatterDanger";

    [DataField]
    public ProtoId<SourcePortPrototype> PortEmergency = "SupermatterEmergency";

    [DataField]
    public ProtoId<SourcePortPrototype> PortDelaminating = "SupermatterDelaminating";

    #endregion

    #region Console-Only Values

    /// <summary>
    /// The power decay of the supermatter, to be displayed on the monitoring console
    /// </summary>
    [DataField][ViewVariables(VVAccess.ReadOnly)][AutoNetworkedField]
    public float PowerLoss;

    /// <summary>
    /// The low temperature healing of the supermatter, to be displayed on the monitoring console
    /// </summary>
    [DataField][ViewVariables(VVAccess.ReadOnly)][AutoNetworkedField]
    public float HeatHealing;

    #endregion
}

public readonly record struct SupermatterGasFact(float TransmitModifier, float WasteModifier, float PowerMixRatio, float HeatResistance)
{
    /// <summary>
    /// Multiplied with the supermatter's power to determine rads
    /// </summary>
    public readonly float TransmitModifier = TransmitModifier;

    /// <summary>
    /// Affects the amount of oxygen and plasma that is released during supermatter reactions, as well as the heat generated
    /// </summary>
    public readonly float WasteModifier = WasteModifier;

    /// <summary>
    /// Affects the amount of power generated by the supermatter
    /// </summary>
    public readonly float PowerMixRatio = PowerMixRatio;

    /// <summary>
    /// Affects the supermatter's resistance to temperature
    /// </summary>
    public readonly float HeatResistance = HeatResistance;
}

public static class SupermatterGasData
{
    public static readonly FrozenDictionary<Gas, SupermatterGasFact> GasData = new Dictionary<Gas, SupermatterGasFact>()
    {
        { Gas.Oxygen,        new(1.5f, 1f,    1f,  1f) },
        { Gas.Nitrogen,      new(0f,   -1.5f, -1f, 1f) },
        { Gas.CarbonDioxide, new(0f,   0.1f,  1f,  1f) },
        { Gas.Plasma,        new(4f,   15f,   1f,  1f) },
        { Gas.Tritium,       new(30f,  10f,   1f,  1f) },
        { Gas.WaterVapor,    new(2f,   12f,   1f,  1f) },
        { Gas.Ammonia,       new(0f,   1f,    1f , 1f) },
        { Gas.NitrousOxide,  new(0f,   -5f,   -1f, 6f) },
        { Gas.Frezon,        new(3f,   -10f,  -1f, 1f) }
    }.ToFrozenDictionary();

    private static float CalculateGasMixModifier(Dictionary<Gas, float> ratios, Func<SupermatterGasFact, float> getModifier)
    {
        float modifier = 0;
        
        foreach (var (gas, ratio) in ratios)
            modifier += ratio * getModifier(GasData[gas]);
        
        return modifier;
    }

    public static float GetTransmitModifiers(Dictionary<Gas, float> ratios)
    {
        return CalculateGasMixModifier(ratios, data => data.TransmitModifier);
    }

    public static float GetMixWastePenalty(Dictionary<Gas, float> ratios)
    {
        return CalculateGasMixModifier(ratios, data => data.WasteModifier);
    }

    public static float GetPowerMixRatios(Dictionary<Gas, float> ratios)
    {
        return CalculateGasMixModifier(ratios, data => data.PowerMixRatio);
    }

    public static float GetHeatResistances(Dictionary<Gas, float> ratios)
    {
        return CalculateGasMixModifier(ratios, data => data.HeatResistance);
    }
}

[Serializable, NetSerializable]
public enum SupermatterStatusType : sbyte
{
    Error = -1,
    Inactive = 0,
    Normal = 1,
    Caution = 2,
    Warning = 3,
    Danger = 4,
    Emergency = 5,
    Delaminating = 6
}

[Serializable, NetSerializable]
public enum SupermatterCrystalState : byte
{
    Normal,
    Glow,
    GlowEmergency,
    GlowDelam
}

[Serializable, NetSerializable]
public enum SupermatterVisuals : byte
{
    Crystal,
    Psy
}

[Serializable, NetSerializable]
public sealed partial class SupermatterDoAfterEvent : SimpleDoAfterEvent;

/// <summary>
/// Raised when the supermatter takes damage, with the amount of damage taken.
/// </summary>
[ByRefEvent]
public record struct SupermatterDamagedEvent;

/// <summary>
/// Raised when the supermatter starts the delamination process.
/// </summary>
[ByRefEvent]
public record struct SupermatterDelaminationStartedEvent;

/// <summary>
/// Raised when the supermatter cancels the delamination process.
/// </summary>
[ByRefEvent]
public record struct SupermatterDelaminationCancelledEvent;

/// <summary>
/// Raised when the supermatter finishes the delamination timer.
/// </summary>
[ByRefEvent]
public record struct SupermatterDelaminationEvent;

/// <summary>
/// Raised when the supermatter should announce its status.
/// </summary>
[ByRefEvent]
public record struct SupermatterAnnouncementEvent;

/// <summary>
/// Raised when the supermatter's status changes.
/// </summary>
[ByRefEvent]
public record struct SupermatterStatusChangedEvent;