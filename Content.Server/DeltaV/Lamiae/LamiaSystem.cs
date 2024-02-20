using Robust.Shared.Physics;
using Content.Shared.Damage;
using Content.Shared.Explosion;
using Content.Shared.Clothing.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Server.Humanoid;
using Content.Shared.Inventory.Events;
using Content.Shared.Tag;
using Content.Shared.Teleportation.Components;
using Content.Shared.Standing;
using Content.Shared.Storage.Components;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics.Components;
using System.Numerics;
using Content.Shared.DeltaV.Lamiae;

namespace Content.Server.DeltaV.Lamiae
{
    public sealed partial class LamiaSystem : EntitySystem
    {
        [Dependency] private readonly SharedJointSystem _jointSystem = default!;
        [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly TagSystem _tagSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly StandingStateSystem _standing = default!;

        [ValidatePrototypeId<TagPrototype>]
        private const string LamiaHardsuitTag = "AllowLamiaHardsuit";

        Queue<(LamiaSegmentComponent segment, EntityUid lamia)> _segments = new();
        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            foreach (var segment in _segments)
            {
                var segmentUid = segment.segment.Owner;
                var attachedUid = segment.segment.AttachedToUid;
                if (!Exists(segmentUid) || !Exists(attachedUid)
                || MetaData(segmentUid).EntityLifeStage > EntityLifeStage.MapInitialized
                || MetaData(attachedUid).EntityLifeStage > EntityLifeStage.MapInitialized
                || Transform(segmentUid).MapID == MapId.Nullspace
                || Transform(attachedUid).MapID == MapId.Nullspace)
                    continue;

                EnsureComp<PhysicsComponent>(segmentUid);
                EnsureComp<PhysicsComponent>(attachedUid); // Hello I hate tests

                var ev = new SegmentSpawnedEvent(segment.lamia);
                RaiseLocalEvent(segmentUid, ev, false);

                if (segment.segment.SegmentNumber == 1)
                {
                    Transform(segmentUid).Coordinates = Transform(attachedUid).Coordinates;
                    var revoluteJoint = _jointSystem.CreateWeldJoint(attachedUid, segmentUid, id: ("Segment" + segment.segment.SegmentNumber + segment.segment.Lamia));
                    revoluteJoint.CollideConnected = false;
                }
                if (segment.segment.SegmentNumber < segment.segment.MaxSegments)
                    Transform(segmentUid).Coordinates = Transform(attachedUid).Coordinates.Offset(new Vector2(0, segment.segment.OffsetSwitching));
                else
                    Transform(segmentUid).Coordinates = Transform(attachedUid).Coordinates.Offset(new Vector2(0, segment.segment.OffsetSwitching));

                var joint = _jointSystem.CreateDistanceJoint(attachedUid, segmentUid, id: ("Segment" + segment.segment.SegmentNumber + segment.segment.Lamia));
                joint.CollideConnected = false;
                joint.Stiffness = 0.2f;
            }
            _segments.Clear();
        }
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<LamiaComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<LamiaComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<LamiaComponent, JointRemovedEvent>(OnJointRemoved);
            SubscribeLocalEvent<LamiaComponent, EntGotRemovedFromContainerMessage>(OnRemovedFromContainer);
            SubscribeLocalEvent<LamiaSegmentComponent, SegmentSpawnedEvent>(OnSegmentSpawned);
            SubscribeLocalEvent<LamiaSegmentComponent, DamageChangedEvent>(HandleDamageTransfer);
            SubscribeLocalEvent<LamiaSegmentComponent, DamageModifyEvent>(HandleSegmentDamage);
            SubscribeLocalEvent<LamiaComponent, InsertIntoEntityStorageAttemptEvent>(OnLamiaStorageInsertAttempt);
            SubscribeLocalEvent<LamiaSegmentComponent, InsertIntoEntityStorageAttemptEvent>(OnSegmentStorageInsertAttempt);
            SubscribeLocalEvent<LamiaComponent, DidEquipEvent>(OnDidEquipEvent);
            SubscribeLocalEvent<LamiaComponent, DidUnequipEvent>(OnDidUnequipEvent);
            SubscribeLocalEvent<LamiaSegmentComponent, BeforeDamageChangedEvent>(OnHitSelf);
            SubscribeLocalEvent<LamiaSegmentComponent, StandAttemptEvent>(TailCantStand);
            SubscribeLocalEvent<LamiaSegmentComponent, GetExplosionResistanceEvent>(OnSnekBoom);
        }

        /// <summary>
        /// Handles transfering marking selections to the tail segments. Every tail marking must be repeated 2 times in order for this script to work.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="component"></param>
        /// <param name="args"></param>
        // TODO: Please for the love of god don't make me write a test to validate that every marking also has its matching segment states.
        // Future contributors will just find out when their game crashes because they didn't make a marking-segment.
        private void OnSegmentSpawned(EntityUid uid, LamiaSegmentComponent component, SegmentSpawnedEvent args)
        {
            component.Lamia = args.Lamia;
            if (component.BulletPassover == true)
                _standing.Down(uid, false);

            if (!TryComp<HumanoidAppearanceComponent>(uid, out var species)) return;
            if (!TryComp<HumanoidAppearanceComponent>(args.Lamia, out var humanoid)) return;
            if (!TryComp<AppearanceComponent>(uid, out var appearance)) return;

            _appearance.SetData(uid, ScaleVisuals.Scale, component.ScaleFactor, appearance);

            if (humanoid.MarkingSet.TryGetCategory(MarkingCategories.Tail, out var tailMarkings))
            {
                foreach (var markings in tailMarkings)
                {
                    var segmentId = species.Species;
                    var markingId = markings.MarkingId;
                    string segmentmarking = $"{markingId}-{segmentId}";
                    _humanoid.AddMarking(uid, segmentmarking, markings.MarkingColors);
                }
            }
        }

        private void OnInit(EntityUid uid, LamiaComponent component, ComponentInit args)
        {
            Math.Clamp(component.NumberOfSegments, 2, 30);
            Math.Clamp(component.TaperOffset, 1, component.NumberOfSegments - 1);
            SpawnSegments(uid, component);
        }

        private void OnShutdown(EntityUid uid, LamiaComponent component, ComponentShutdown args)
        {
            foreach (var segment in component.Segments)
            {
                QueueDel(segment);
            }

            component.Segments.Clear();
        }

        private void OnJointRemoved(EntityUid uid, LamiaComponent component, JointRemovedEvent args)
        {
            if (!component.Segments.Contains(args.OtherBody.Owner))
                return;

            if (HasComp<PortalTimeoutComponent>(uid)) return;

            foreach (var segment in component.Segments)
                QueueDel(segment);

            component.Segments.Clear();
        }

        private void OnRemovedFromContainer(EntityUid uid, LamiaComponent component, EntGotRemovedFromContainerMessage args)
        {
            if (component.Segments.Count != 0)
            {
                foreach (var segment in component.Segments)
                QueueDel(segment);
                component.Segments.Clear();
            }

            SpawnSegments(uid, component);
        }

        private void HandleSegmentDamage(EntityUid uid, LamiaSegmentComponent component, DamageModifyEvent args)
        {
            args.Damage = args.Damage / component.DamageModifyFactor;
        }
        private void HandleDamageTransfer(EntityUid uid, LamiaSegmentComponent component, DamageChangedEvent args)
        {
            if (args.DamageDelta == null) return;
            _damageableSystem.TryChangeDamage(component.Lamia, args.DamageDelta);
        }

        private void OnHitSelf(EntityUid uid, LamiaSegmentComponent component, ref BeforeDamageChangedEvent args)
        {
            if (args.Origin == component.Lamia)
            {
                args.Cancelled = true;
            }
        }

        private void TailCantStand(EntityUid uid, LamiaSegmentComponent component, StandAttemptEvent args)
        {
            if (component.BulletPassover == true)
                args.Cancel();
        }

        public void SpawnSegments(EntityUid uid, LamiaComponent component)
        {
            int i = 1;
            var addTo = uid;
            while (i < component.NumberOfSegments + 1)
            {
                var segment = AddSegment(addTo, uid, component, i);
                addTo = segment;
                i++;
            }
        }

        private EntityUid AddSegment(EntityUid uid, EntityUid lamia, LamiaComponent lamiaComponent, int segmentNumber)
        {
            EnsureComp<LamiaSegmentComponent>(uid, out var segmentComponent);
            segmentComponent.MaxSegments = lamiaComponent.NumberOfSegments;
            segmentComponent.BulletPassover = lamiaComponent.BulletPassover;
            segmentComponent.Lamia = lamia;
            segmentComponent.AttachedToUid = uid;
            segmentComponent.DamageModifierConstant = lamiaComponent.NumberOfSegments * lamiaComponent.DamageModifierOffset;
            float taperConstant = lamiaComponent.NumberOfSegments - lamiaComponent.TaperOffset;
            float damageModifyCoefficient = segmentComponent.DamageModifierConstant / lamiaComponent.NumberOfSegments;
            segmentComponent.DamageModifyFactor = segmentComponent.DamageModifierConstant * damageModifyCoefficient;
            segmentComponent.ExplosiveModifyFactor = 1 / segmentComponent.DamageModifyFactor / (lamiaComponent.NumberOfSegments * lamiaComponent.ExplosiveModifierOffset);

            EntityUid segment;
            if (segmentNumber == 1)
                segment = EntityManager.SpawnEntity(lamiaComponent.InitialSegmentId, Transform(uid).Coordinates);
            else
                segment = EntityManager.SpawnEntity(lamiaComponent.SegmentId, Transform(uid).Coordinates);
            if (segmentNumber >= taperConstant && lamiaComponent.UseTaperSystem == true)
            {
                segmentComponent.OffsetSwitching = lamiaComponent.StaticOffset * MathF.Pow(lamiaComponent.OffsetConstant, segmentNumber - taperConstant);
                segmentComponent.ScaleFactor = lamiaComponent.StaticScale * MathF.Pow(1f / lamiaComponent.OffsetConstant, segmentNumber - taperConstant);
            } else
            {
                segmentComponent.OffsetSwitching = lamiaComponent.StaticOffset;
                segmentComponent.ScaleFactor = lamiaComponent.StaticScale;
            }
            if (segmentNumber % 2 != 0)
            {
                segmentComponent.OffsetSwitching *= -1;
            }

            segmentComponent.Owner = segment;
            segmentComponent.SegmentNumber = segmentNumber;

            _segments.Enqueue((segmentComponent, lamia));
            lamiaComponent.Segments.Add(segmentComponent.Owner);
            return segment;
        }

        private void OnLamiaStorageInsertAttempt(EntityUid uid, LamiaComponent comp, ref InsertIntoEntityStorageAttemptEvent args)
        {
            args.Cancelled = true;
        }

        private void OnSegmentStorageInsertAttempt(EntityUid uid, LamiaSegmentComponent comp, ref InsertIntoEntityStorageAttemptEvent args)
        {
            args.Cancelled = true;
        }

        private void OnDidEquipEvent(EntityUid equipee, LamiaComponent component, DidEquipEvent args)
        {
            if (!TryComp<ClothingComponent>(args.Equipment, out var clothing)) return;
            if (args.Slot == "outerClothing" && _tagSystem.HasTag(args.Equipment, LamiaHardsuitTag))
            {
                foreach (var uid in component.Segments)
                {
                    if (!TryComp<AppearanceComponent>(uid, out var appearance)) return;
                    _appearance.SetData(uid, LamiaSegmentVisualLayers.Armor, true, appearance);
                    if (clothing.RsiPath == null) return;
                    _appearance.SetData(uid, LamiaSegmentVisualLayers.ArmorRsi, clothing.RsiPath, appearance);
                }
            }
        }

        private void OnSnekBoom(EntityUid uid, LamiaSegmentComponent component, ref GetExplosionResistanceEvent args)
        {
            args.DamageCoefficient = component.ExplosiveModifyFactor;
        }

        private void OnDidUnequipEvent(EntityUid equipee, LamiaComponent component, DidUnequipEvent args)
        {
            if (args.Slot == "outerClothing" && _tagSystem.HasTag(args.Equipment, LamiaHardsuitTag))
            {
                foreach (var uid in component.Segments)
                {
                    if (!TryComp<AppearanceComponent>(uid, out var appearance)) return;
                    _appearance.SetData(uid, LamiaSegmentVisualLayers.Armor, false, appearance);
                }
            }
        }
    }
}
