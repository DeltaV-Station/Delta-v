using Robust.Shared.Physics;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Maths;
using System.Numerics;
using Content.Shared.Nyanotrasen.Lamiae;

namespace Content.Shared.Nyanotrasen.Lamiae
{
    public partial class SharedLamiaSystem : EntitySystem
    {
        [Dependency] private readonly SharedJointSystem _jointSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypes = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;

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
                if (segment.segment.SegmentNumber < 32)
                    Transform(segmentUid).Coordinates = Transform(attachedUid).Coordinates.Offset(new Vector2(0f, 0.15f));
                else
                    Transform(segmentUid).Coordinates = Transform(attachedUid).Coordinates.Offset(new Vector2(0, 0.1f));

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
            SubscribeLocalEvent<LamiaSegmentComponent, DamageModifyEvent>(HandleSegmentDamage);
        }

        private void OnInit(EntityUid uid, LamiaComponent component, ComponentInit args)
        {
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
            args.Damage.DamageDict["Radiation"] = Shared.FixedPoint.FixedPoint2.Zero;
            _damageableSystem.TryChangeDamage(component.Lamia, args.Damage);

            args.Damage *= 0;
        }

        private void SpawnSegments(EntityUid uid, LamiaComponent component)
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
            LamiaSegmentComponent segmentComponent = new();
            segmentComponent.AttachedToUid = uid;
            EntityUid segment;
            if (segmentNumber == 1)
                segment = EntityManager.SpawnEntity("LamiaInitialSegment", Transform(uid).Coordinates);
            else if (segmentNumber == lamiaComponent.NumberOfSegments)
                segment = EntityManager.SpawnEntity("LamiaSegmentEnd", Transform(uid).Coordinates);
            else
                segment = EntityManager.SpawnEntity("LamiaSegment", Transform(uid).Coordinates);

            segmentComponent.Owner = segment;
            segmentComponent.SegmentNumber = segmentNumber;
            EntityManager.AddComponent(segment, segmentComponent, true);
            _segments.Enqueue((segmentComponent, lamia));
            lamiaComponent.Segments.Add(segmentComponent.Owner);
            return segment;
        }
    }
}
