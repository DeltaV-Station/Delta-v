using Content.Shared._DV.CCVars;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Configuration;

namespace Content.Shared._DV.Traitor;

/// <summary>
/// Provides API for ransoming entities.
/// </summary>
public sealed class RansomSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    private float _mobBase;
    private float _humanoidBase;
    private float _deadMod;
    private float _critMod;
    private List<RansomData> _ransoms = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RansomComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<RansomComponent, MoveEvent>(OnMove);

        Subs.CVar(_cfg, DCCVars.MobRansom, n => _mobBase = n, true);
        Subs.CVar(_cfg, DCCVars.HumanoidRansom, n => _humanoidBase = n, true);
        Subs.CVar(_cfg, DCCVars.RansomDeadModifier, n => _deadMod = n, true);
        Subs.CVar(_cfg, DCCVars.RansomCritModifier, n => _critMod = n, true);
    }

    private void OnMapInit(Entity<RansomComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.Map = Transform(ent).MapID;
    }

    private void OnMove(Entity<RansomComponent> ent, ref MoveEvent args)
    {
        // remove ransom when its paid, or if they sneak a fulton/whatever into the jail, or get admin help
        if (Transform(ent).MapID != ent.Comp.Map)
            RemCompDeferred<RansomComponent>(ent);
    }

    /// <summary>
    /// Ransoms an entity and returns the price.
    /// </summary>
    public int RansomEntity(EntityUid uid)
    {
        var ransom = GetRansom(uid);
        EnsureComp<RansomComponent>(uid).Ransom = ransom;
        return ransom;
    }

    /// <summary>
    /// Get the price for an entity's ransom.
    /// </summary>
    public int GetRansom(EntityUid uid)
    {
        var ransom = HasComp<HumanoidAppearanceComponent>(uid)
            ? _humanoidBase
            : _mobBase;

        // hostages are more valuable alive than dead, shocker
        if (_mob.IsDead(uid))
            ransom *= _deadMod;
        else if (_mob.IsCritical(uid))
            ransom *= _critMod;

        // multiply by the job's ransom value
        if (_mind.GetMind(uid) is {} mind && _job.MindTryGetJob(mind, out var job))
            ransom *= job.RansomModifier;

        return (int) ransom;
    }

    /// <summary>
    /// Get a list of all ransomed mobs for sending to cargo request console UI.
    /// It is reused, do not modify it.
    /// </summary>
    public List<RansomData> GetRansoms()
    {
        _ransoms.Clear();
        var query = EntityQueryEnumerator<RansomComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var ent = GetNetEntity(uid);
            var name = Name(uid);
            var price = comp.Ransom;
            _ransoms.Add(new RansomData(ent, name, price));
        }
        return _ransoms;
    }
}
