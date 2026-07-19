using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.WashingMachine;

/// <summary>
/// This defines a machine with entityStorage capable of cleaning reagent stains on clothing.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class WashingMachineComponent : Component
{
    /// <summary>
    /// The duration of the washing process that determines <see cref="WashFinishTime"/>.
    /// </summary>
    [DataField]
    public TimeSpan WashTime = TimeSpan.FromSeconds(5);

    /// <summary>
    /// When the washing process is finished.
    /// </summary>
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan? WashFinishTime;

    /// <summary>
    /// The cooldown after each washing step for the next one.
    /// </summary>
    [DataField]
    public TimeSpan WashingStepCooldown = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The next time when washing is calculated (Damaging entities, spraying with water, etc.)
    /// </summary>
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan? NextWashingStep;

    /// <summary>
    /// The cooldown length after <see cref="WashFinishTime"/> to determine <see cref="NextWashAllowed"/>.
    /// </summary>
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(6);

    /// <summary>
    /// The time when the washing machine can wash again after finishing a load of laundry.
    /// </summary>
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan? NextWashAllowed;

    /// <summary>
    /// The sound the washing machine makes during the washing process.
    /// </summary>
    [DataField]
    public SoundSpecifier? WashLoopSound;

    /// <summary>
    /// The sound the washing machine makes after it finishes.
    /// </summary>
    [DataField]
    public SoundSpecifier? WashFinishedSound;

    /// <summary>
    /// The current State of the washing machine, used for appearance.
    /// </summary>
    [DataField, AutoNetworkedField]
    public WashingMachineState State = WashingMachineState.Idle;

    /// <summary>
    /// The current audio being played.
    /// </summary>
    /// <remarks>We save it so we can stop the looping audio when the process finishes.</remarks>
    public EntityUid? AudioStream;

    /// <summary>
    /// The chance of a thump sound to occur whenever something that isn't clothing is washed.
    /// </summary>
    [DataField]
    public float ThumpSoundChance = 0.8f;

    /// <summary>
    /// The reagent to spray on entities inside the active washing machine.
    /// </summary>
    [DataField]
    public string SprayReagent = "Water";

    /// <summary>
    /// The amount of reagent to spray on entities inside the active washing machine.
    /// </summary>
    [DataField]
    public float ReagentSprayAmount = 10.0f;

    /// <summary>
    /// The chance to spray the reagent on entities inside per step.
    /// </summary>
    [DataField]
    public float ReagentSprayChance = 1.0f;

    /// <summary>
    /// The damage dealt to entities within an active washing machine every <see cref="WashingStepCooldown"/>.
    /// </summary>
    [DataField]
    public float EntityBluntDamage = 6.0f;

    /// <summary>
    /// The damage done to the washing machine itself upon finishing the washing process, multiplied by <see cref="WashTime"/>.
    /// </summary>
    [DataField]
    public float SelfDamage = 5.0f;
}

[Serializable, NetSerializable]
public enum WashingMachineState : byte
{
    Idle,
    Washing,
}

[Serializable, NetSerializable]
public enum WashingMachineVisuals : byte
{
    State
}
