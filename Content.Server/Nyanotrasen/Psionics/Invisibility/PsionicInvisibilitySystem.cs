using Content.Shared.Abilities.Psionics;
using Content.Server.Abilities.Psionics;
using Content.Shared.Eye;
using Content.Shared.NPC.Systems;
using Robust.Shared.Containers;
using Robust.Server.GameObjects;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.Psionics
{
    public sealed class PsionicInvisibilitySystem : EntitySystem
    {
        [Dependency] private readonly VisibilitySystem _visibility = default!;
        [Dependency] private readonly PsionicInvisibilityPowerSystem _invisSystem = default!;
        [Dependency] private readonly NpcFactionSystem _faction = default!;
        [Dependency] private readonly SharedEyeSystem _eye = default!;

        private static readonly ProtoId<NpcFactionPrototype> PsionicInterloperProtoId = "PsionicInterloper";
        private static readonly ProtoId<NpcFactionPrototype> GlimmerMonsterProtoId = "GlimmerMonster";


        public override void Initialize()
        {
            base.Initialize();
            /// Masking
            SubscribeLocalEvent<PotentialPsionicComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<PsionicInsulationComponent, ComponentInit>(OnInsulInit);
            SubscribeLocalEvent<PsionicInsulationComponent, ComponentShutdown>(OnInsulShutdown);
            SubscribeLocalEvent<EyeComponent, ComponentInit>(OnEyeInit);

            /// Layer
            SubscribeLocalEvent<PsionicallyInvisibleComponent, ComponentInit>(OnInvisInit);
            SubscribeLocalEvent<PsionicallyInvisibleComponent, ComponentShutdown>(OnInvisShutdown);

            // PVS Stuff
            SubscribeLocalEvent<PsionicallyInvisibleComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
            SubscribeLocalEvent<PsionicallyInvisibleComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
        }

        private void OnInit(EntityUid uid, PotentialPsionicComponent component, ComponentInit args)
        {
            SetCanSeePsionicInvisiblity(uid, false);
        }

        private void OnInsulInit(EntityUid uid, PsionicInsulationComponent component, ComponentInit args)
        {
            if (!HasComp<PotentialPsionicComponent>(uid))
                return;

            if (HasComp<PsionicInvisibilityUsedComponent>(uid))
                _invisSystem.ToggleInvisibility(uid);

            if (_faction.IsMember(uid, PsionicInterloperProtoId))
            {
                component.SuppressedFactions.Add(PsionicInterloperProtoId);
                _faction.RemoveFaction(uid, PsionicInterloperProtoId);
            }

            if (_faction.IsMember(uid, GlimmerMonsterProtoId))
            {
                component.SuppressedFactions.Add(GlimmerMonsterProtoId);
                _faction.RemoveFaction(uid, GlimmerMonsterProtoId);
            }

            SetCanSeePsionicInvisiblity(uid, true);
        }

        private void OnInsulShutdown(EntityUid uid, PsionicInsulationComponent component, ComponentShutdown args)
        {
            if (!HasComp<PotentialPsionicComponent>(uid))
                return;

            SetCanSeePsionicInvisiblity(uid, false);

            if (!HasComp<PsionicComponent>(uid))
            {
                component.SuppressedFactions.Clear();
                return;
            }

            foreach (var faction in component.SuppressedFactions)
            {
                _faction.AddFaction(uid, faction);
            }
            component.SuppressedFactions.Clear();
        }

        private void OnInvisInit(EntityUid uid, PsionicallyInvisibleComponent component, ComponentInit args)
        {
            var visibility = EnsureComp<VisibilityComponent>(uid);
            var ent = (uid, visibility);

            _visibility.AddLayer(ent, (int) VisibilityFlags.PsionicInvisibility, false);
            _visibility.RemoveLayer(ent, (int) VisibilityFlags.Normal, false);
            _visibility.RefreshVisibility(ent);
        }


        private void OnInvisShutdown(EntityUid uid, PsionicallyInvisibleComponent component, ComponentShutdown args)
        {
            if (!TryComp<VisibilityComponent>(uid, out var visibility))
                return;

            var ent = (uid, visibility);
            _visibility.RemoveLayer(ent, (int) VisibilityFlags.PsionicInvisibility, false);
            _visibility.AddLayer(ent, (int) VisibilityFlags.Normal, false);
            _visibility.RefreshVisibility(ent);
        }

        private void OnEyeInit(EntityUid uid, EyeComponent component, ComponentInit args)
        {
            //SetCanSeePsionicInvisiblity(uid, true); //JJ Comment - Not allowed to modifies .yml on spawn any longer. See UninitializedSaveTest.
        }
        private void OnEntInserted(EntityUid uid, PsionicallyInvisibleComponent component, EntInsertedIntoContainerMessage args)
        {
            DirtyEntity(args.Entity);
        }

        private void OnEntRemoved(EntityUid uid, PsionicallyInvisibleComponent component, EntRemovedFromContainerMessage args)
        {
            DirtyEntity(args.Entity);
        }

        public void SetCanSeePsionicInvisiblity(EntityUid uid, bool set)
        {
            if (set == true)
            {
                if (EntityManager.TryGetComponent(uid, out EyeComponent? eye))
                {
                    _eye.SetVisibilityMask(uid, eye.VisibilityMask | (int) VisibilityFlags.PsionicInvisibility, eye);
                }
            } else
            {
                if (EntityManager.TryGetComponent(uid, out EyeComponent? eye))
                {
                    _eye.SetVisibilityMask(uid, eye.VisibilityMask & ~ (int) VisibilityFlags.PsionicInvisibility, eye);
                }
            }
        }
    }
}
