using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Server.Psionics
{
    /// <summary>
    /// Allows an entity to become psionically invisible when touching certain entities.
    /// </summary>
    public sealed class PsionicInvisibleContactsSystem : EntitySystem
    {
        [Dependency] private readonly SharedStealthSystem _stealth = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PsionicInvisibleContactsComponent, StartCollideEvent>(OnEntityEnter);
            SubscribeLocalEvent<PsionicInvisibleContactsComponent, EndCollideEvent>(OnEntityExit);

            UpdatesAfter.Add(typeof(SharedPhysicsSystem));
        }

        private void OnEntityEnter(EntityUid uid, PsionicInvisibleContactsComponent component, ref StartCollideEvent args)
        {
            var otherUid = args.OtherEntity;
            var ourEntity = args.OurEntity;

            if (_whitelistSystem.IsWhitelistFail(component.Whitelist, otherUid))
                return;

            // This will go up twice per web hit, since webs also have a flammable fixture.
            // It goes down twice per web exit, so everything's fine.
            ++component.Stages;

            if (HasComp<PsionicallyInvisibleComponent>(ourEntity))
                return;

            EnsureComp<PsionicallyInvisibleComponent>(ourEntity);
            var stealth = EnsureComp<StealthComponent>(ourEntity);
            _stealth.SetVisibility(ourEntity, 0.66f, stealth);
        }

        private void OnEntityExit(EntityUid uid, PsionicInvisibleContactsComponent component, ref EndCollideEvent args)
        {
            var otherUid = args.OtherEntity;
            var ourEntity = args.OurEntity;

            if (_whitelistSystem.IsWhitelistFail(component.Whitelist, otherUid))
                return;

            if (!HasComp<PsionicallyInvisibleComponent>(ourEntity))
                return;

            if (--component.Stages > 0)
                return;

            RemComp<PsionicallyInvisibleComponent>(ourEntity);
            var stealth = EnsureComp<StealthComponent>(ourEntity);
            // Just to be sure...
            _stealth.SetVisibility(ourEntity, 1f, stealth);

            RemComp<StealthComponent>(ourEntity);
        }
    }
}
