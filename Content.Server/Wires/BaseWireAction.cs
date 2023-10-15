using Robust.Shared.Random;
using Content.Server.Electrocution;
using Content.Server.Power.EntitySystems;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Wires;

namespace Content.Server.Wires;

/// <summary><see cref="IWireAction" /></summary>
[ImplicitDataDefinitionForInheritors]
public abstract partial class BaseWireAction : IWireAction
{
    private ISharedAdminLogManager _adminLogger = default!;

    /// <summary>
    ///     The loc-string of the text that gets returned by <see cref="GetStatusLightData(Wire)"/>. Also used for admin logging.
    /// </summary>
    [DataField("name")]
    public abstract string Name { get; set; }

    /// <summary>
    ///     Default color that gets returned by <see cref="GetStatusLightData(Wire)"/>.
    /// </summary>
    [DataField("color")]
    public abstract Color Color { get; set; }

    /// <summary>
    ///     If true, the default behavior of <see cref="GetStatusLightData(Wire)"/> will return an off-light when the
    ///     wire owner is not powered.
    /// </summary>
    [DataField("lightRequiresPower")]
    public virtual bool LightRequiresPower { get; set; } = true;

    /// <summary>
    ///     Nyanotrasen - The chance that the user is shocked when tampering with the wire: cutting, pulsing, or mending it.
    /// </summary>
    [DataField("shockChance")]
    public float ShockChance = 0.55f;

    /// <summary>
    ///     Nyanotrasen - How much damage the user takes when tampering.
    /// </summary>
    [DataField("shockDamage")]
    public int ShockDamage = 15;

    /// <summary>
    ///     Nyanotrasen - How long the user is stunned after a failed tamper attempt.
    /// </summary>
    [DataField("shockStunTime")]
    public TimeSpan ShockStunTime = TimeSpan.FromSeconds(3f);

    public virtual StatusLightData? GetStatusLightData(Wire wire)
    {
        if (LightRequiresPower && !IsPowered(wire.Owner))
            return new StatusLightData(Color, StatusLightState.Off, Loc.GetString(Name));

        var state = GetLightState(wire);
        return state == null
            ? null
            : new StatusLightData(Color, state.Value, Loc.GetString(Name));
    }

    public virtual StatusLightState? GetLightState(Wire wire) => null;

    public IEntityManager EntityManager = default!;
    public IRobustRandom Random = default!;
    public WiresSystem WiresSystem = default!;
    public ElectrocutionSystem ElectrocutionSystem = default!;

    // not virtual so implementors are aware that they need a nullable here
    public abstract object? StatusKey { get; }

    // ugly, but IoC doesn't work during deserialization
    public virtual void Initialize()
    {
        EntityManager = IoCManager.Resolve<IEntityManager>();
        _adminLogger = IoCManager.Resolve<ISharedAdminLogManager>();
        Random = IoCManager.Resolve<IRobustRandom>();

        WiresSystem = EntityManager.EntitySysManager.GetEntitySystem<WiresSystem>();
        ElectrocutionSystem = EntityManager.EntitySysManager.GetEntitySystem<ElectrocutionSystem>();
    }

    public virtual bool AddWire(Wire wire, int count) => count == 1;
    public virtual bool Cut(EntityUid user, Wire wire) => !TryShockUser(user, wire, "cutting") && Log(user, wire, "cut"); // Nyanotrasen - Tactical hacking
    public virtual bool Mend(EntityUid user, Wire wire) => !TryShockUser(user, wire, "mending") && Log(user, wire, "mended"); // Nyanotrasen - Tactical hacking
    public virtual void Pulse(EntityUid user, Wire wire) // Nyanotrasen - Tactical hacking
    {
        if (!TryShockUser(user, wire, "pulsing"))
            Log(user, wire, "pulsed");
    }

    /// <summary>
    /// Nyanotrasen - Returns true if the user has been shocked.
    /// </summary>
    private bool TryShockUser(EntityUid user, Wire wire, string verb)
    {
        if (!IsPowered(wire.Owner))
            return false;

        if (!Random.Prob(ShockChance))
            return false;

        var shocked = ElectrocutionSystem.TryDoElectrocution(user, wire.Owner, ShockDamage, ShockStunTime, false);

        if (shocked)
        {
            var player = EntityManager.ToPrettyString(user);
            var owner = EntityManager.ToPrettyString(wire.Owner);
            var name = Loc.GetString(Name);
            var color = wire.Color.Name();
            var action = GetType().Name;

            _adminLogger.Add(LogType.WireHacking, LogImpact.Medium, $"{player} shocked by {owner} when {verb} {color} {name} wire ({action})");
        }

        return shocked;
    }

    private bool Log(EntityUid user, Wire wire, string verb)
    {
        var player = EntityManager.ToPrettyString(user);
        var owner = EntityManager.ToPrettyString(wire.Owner);
        var name = Loc.GetString(Name);
        var color = wire.Color.Name();
        var action = GetType().Name;

        // logs something like "... mended red POWR wire (PowerWireAction) in ...."
        _adminLogger.Add(LogType.WireHacking, LogImpact.Medium, $"{player} {verb} {color} {name} wire ({action}) in {owner}");
        return true;
    }

    public virtual void Update(Wire wire)
    {
    }

    /// <summary>
    ///     Utility function to check if this given entity is powered.
    /// </summary>
    /// <returns>true if powered, false otherwise</returns>
    protected bool IsPowered(EntityUid uid)
    {
        return WiresSystem.IsPowered(uid, EntityManager);
    }
}
