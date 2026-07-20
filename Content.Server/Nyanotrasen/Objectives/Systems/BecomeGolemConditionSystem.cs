using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems
{
    public sealed class BecomeGolemConditionSystem : EntitySystem
    {
        private EntityQuery<MetaDataComponent> _metaQuery;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BecomeGolemConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        }

        private void OnGetProgress(EntityUid uid, BecomeGolemConditionComponent comp, ref ObjectiveGetProgressEvent args)
        {
            args.Progress = GetProgress(args.Mind);
        }

        private float GetProgress(MindComponent mind)
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            if (!_metaQuery.TryGetComponent(mind.OwnedEntity, out var meta))
                return 0;
            /*EntityManager.TryGetComponent<GolemComponent>(mind.CurrentEntity, out var GolemComp);
            if(GolemComp)
                return 1; TODO: Add this code back once Golems are implemented. */
            return 0;
        }
    }
}
