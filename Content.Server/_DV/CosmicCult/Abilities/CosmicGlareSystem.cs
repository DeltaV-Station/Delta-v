using System.Linq;
using Content.Server._EE.Silicon.WeldingHealable;
using Content.Server.Bible.Components;
using Content.Server.Flash;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Server.Stunnable;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Effects;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicGlareSystem : EntitySystem
{
    [Dependency] private readonly CosmicCultSystem _cult = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly FlashSystem _flash = default!;
    [Dependency] private readonly PoweredLightSystem _poweredLight = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly SharedCosmicCultSystem _cosmicCult = default!;
    [Dependency] private readonly SharedInteractionSystem _interact = default!;

    private HashSet<Entity<PoweredLightComponent>> _lights = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicCultComponent, EventCosmicGlare>(OnCosmicGlare);
    }

    private void OnCosmicGlare(Entity<CosmicCultComponent> uid, ref EventCosmicGlare args)
    {
        _audio.PlayPvs(uid.Comp.GlareSFX, uid);
        Spawn(uid.Comp.GlareVFX, Transform(uid).Coordinates);
        _cult.MalignEcho(uid);
        args.Handled = true;

        _lights.Clear();
        _lookup.GetEntitiesInRange<PoweredLightComponent>(Transform(uid).Coordinates, uid.Comp.CosmicGlareRange, _lights);

        foreach (var entity in _lights)
            _poweredLight.TryDestroyBulb(entity);

        var targetFilter = Filter.Pvs(uid).RemoveWhere(player =>
        {
            if (player.AttachedEntity == null)
                return true;

            var ent = player.AttachedEntity.Value;
            if (!HasComp<MobStateComponent>(ent) || _cosmicCult.EntityIsCultist(ent) || HasComp<BibleUserComponent>(ent))
                return true;

            return !_interact.InRangeUnobstructed((uid, Transform(uid)), (ent, Transform(ent)), range: 0, collisionMask: CollisionGroup.Impassable);
        });

        var targets = new HashSet<NetEntity>(targetFilter.RemovePlayerByAttachedEntity(uid).Recipients.Select(ply => GetNetEntity(ply.AttachedEntity!.Value)));
        foreach (var target in targets)
        {
            var targetEnt = GetEntity(target);

            _flash.Flash(targetEnt, uid, args.Action, (float)uid.Comp.CosmicGlareDuration.TotalMilliseconds, uid.Comp.CosmicGlarePenalty, false, false, uid.Comp.CosmicGlareStun);

            if (HasComp<WeldingHealableComponent>(targetEnt)) //This component is used exclusively by IPCs and borgs, so we use it here to target 'em specifically.
            {
                _stun.TryParalyze(targetEnt, uid.Comp.CosmicGlareDuration / 2, true);
            }

            _color.RaiseEffect(Color.CadetBlue, new List<EntityUid>() { targetEnt }, Filter.Pvs(targetEnt, entityManager: EntityManager));
        }
    }
}
