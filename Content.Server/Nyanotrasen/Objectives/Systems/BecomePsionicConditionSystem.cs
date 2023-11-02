using Content.Shared.Abilities.Psionics;
using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems
{
    public sealed class BecomePsionicConditionSystem : EntitySystem
    {
        private EntityQuery<MetaDataComponent> _metaQuery;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BecomePsionicConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        }

        private void OnGetProgress(EntityUid uid, BecomePsionicConditionComponent comp, ref ObjectiveGetProgressEvent args)
        {
            args.Progress = GetProgress(args.Mind);
        }

        private float GetProgress(MindComponent mind)
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            if (HasComp<PsionicComponent>(mind.CurrentEntity))
                return 1;
            return 0;
        }
    }
}
