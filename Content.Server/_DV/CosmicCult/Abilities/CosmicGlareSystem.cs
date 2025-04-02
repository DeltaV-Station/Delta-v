using System.Linq;
using Content.Server.Bible.Components;
using Content.Server.Flash;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Effects;
using Content.Shared.Humanoid;
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
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly SharedInteractionSystem _interact = default!;

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
        var entities = _lookup.GetEntitiesInRange(Transform(uid).Coordinates, uid.Comp.CosmicGlareRange);
        entities.RemoveWhere(entity => !HasComp<PoweredLightComponent>(entity));
        foreach (var entity in entities)
            _poweredLight.TryDestroyBulb(entity);

        var targetFilter = Filter.Pvs(uid).RemoveWhere(player =>
        {
            if (player.AttachedEntity == null)
                return true;
            var ent = player.AttachedEntity.Value;
            if (!HasComp<MobStateComponent>(ent) || !HasComp<HumanoidAppearanceComponent>(ent) || HasComp<CosmicCultComponent>(ent) || HasComp<BibleUserComponent>(ent))
                return true;
            return !_interact.InRangeUnobstructed((uid, Transform(uid)), (ent, Transform(ent)), range: 0, collisionMask: CollisionGroup.Impassable);
        });
        var targets = new HashSet<NetEntity>(targetFilter.RemovePlayerByAttachedEntity(uid).Recipients.Select(ply => GetNetEntity(ply.AttachedEntity!.Value)));
        foreach (var target in targets)
        {
            _flash.Flash(GetEntity(target), uid, args.Action, uid.Comp.CosmicGlareDuration, uid.Comp.CosmicGlarePenalty, false, false, uid.Comp.CosmicGlareStun);
            _color.RaiseEffect(Color.CadetBlue, new List<EntityUid>() { GetEntity(target) }, Filter.Pvs(GetEntity(target), entityManager: EntityManager));
        }
    }
}
