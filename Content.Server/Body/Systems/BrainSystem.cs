using Content.Server.Body.Components;
using Content.Server.Ghost.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Pointing;
// DeltaV Start
using Content.Shared.Examine;
using Content.Server.Traits.Assorted;
using Robust.Shared.Utility;
// DeltaV End

namespace Content.Server.Body.Systems
{
    public sealed class BrainSystem : EntitySystem
    {
        [Dependency] private readonly SharedMindSystem _mindSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BrainComponent, OrganAddedToBodyEvent>((uid, _, args) => HandleMind(args.Body, uid));
            SubscribeLocalEvent<BrainComponent, OrganRemovedFromBodyEvent>((uid, _, args) => HandleMind(uid, args.OldBody));
            SubscribeLocalEvent<BrainComponent, PointAttemptEvent>(OnPointAttempt);
            SubscribeLocalEvent<UnborgableComponent, ExaminedEvent>(OnExamined); // DeltaV
        }

        private void HandleMind(EntityUid newEntity, EntityUid oldEntity)
        {
            if (TerminatingOrDeleted(newEntity) || TerminatingOrDeleted(oldEntity))
                return;

            EnsureComp<MindContainerComponent>(newEntity);
            EnsureComp<MindContainerComponent>(oldEntity);

            var ghostOnMove = EnsureComp<GhostOnMoveComponent>(newEntity);
            if (HasComp<BodyComponent>(newEntity))
                ghostOnMove.MustBeDead = true;

            if (HasComp<UnborgableComponent>(oldEntity)) // DeltaV
                EnsureComp<UnborgableComponent>(newEntity);

            if (!_mindSystem.TryGetMind(oldEntity, out var mindId, out var mind))
                return;

            _mindSystem.TransferTo(mindId, newEntity, mind: mind);
        }

        private void OnExamined(Entity<UnborgableComponent> ent, ref ExaminedEvent args) //DeltaV
        {
            var msg = new FormattedMessage();
            msg.AddMarkupPermissive("[color=red]This brain is damaged beyond use.[/color]");

            args.PushMessage(msg, 1);
        }

        private void OnPointAttempt(Entity<BrainComponent> ent, ref PointAttemptEvent args)
        {
            args.Cancel();
        }
    }
}
