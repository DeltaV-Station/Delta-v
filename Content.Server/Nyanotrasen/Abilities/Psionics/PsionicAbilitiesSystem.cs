using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Server.EUI;
using Content.Server.Psionics;
using Content.Server.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.StatusEffect;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Examine;
using static Content.Shared.Examine.ExamineSystemShared;

namespace Content.Server.Abilities.Psionics
{
    public sealed class PsionicAbilitiesSystem : EntitySystem
    {
        [Dependency] private readonly IComponentFactory _componentFactory = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly EuiManager _euiManager = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
        [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly MindSystem _mindSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popups = default!;

        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Adds a psychic power once a character rolls one. This used to be a system you have to select for. However the opt-in is no longer the text window, but is now done at character creation.
        /// This is going to get removed when I reach Part 3 of my reworks, when I touch upon the GlimmerSystem itself and overhaul how players get powers.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="component"></param>
        /// <param name="args"></param>
        public void AddPsionics(EntityUid uid)
        {
            if (Deleted(uid))
                return;

            if (HasComp<PsionicComponent>(uid))
                return;

            AddRandomPsionicPower(uid);
        }
        public void AddRandomPsionicPower(EntityUid uid)
        {
            EnsureComp<PsionicComponent>(uid, out var psionic);

            if (!_prototypeManager.TryIndex<WeightedRandomPrototype>("RandomPsionicPowerPool", out var pool))
            {
                Logger.Error("Can't index the random psionic power pool!");
                return;
            }

            // uh oh, stinky!
            var newComponent = (Component) _componentFactory.GetComponent(pool.Pick());
            newComponent.Owner = uid;

            EntityManager.AddComponent(uid, newComponent);

            _glimmerSystem.Glimmer += _random.Next((int) MathF.Round(psionic.Amplification * psionic.Dampening * 1), (int) MathF.Round(psionic.Amplification * psionic.Dampening * 5));
        }

        public void RemovePsionics(EntityUid uid)
        {
            if (!TryComp<PsionicComponent>(uid, out var psionic))
                return;

            if (!psionic.Removable)
                return;

            if (!_prototypeManager.TryIndex<WeightedRandomPrototype>("RandomPsionicPowerPool", out var pool))
            {
                Logger.Error("Can't index the random psionic power pool!");
                return;
            }

            foreach (var compName in pool.Weights.Keys)
            {
                // component moment
                var comp = _componentFactory.GetComponent(compName);
                if (EntityManager.TryGetComponent(uid, comp.GetType(), out var psionicPower))
                    RemComp(uid, psionicPower);
            }
            if (psionic.PsionicAbility != null){
                _actionsSystem.TryGetActionData( psionic.PsionicAbility, out var psiAbility );
                if (psiAbility != null){
                    _actionsSystem.RemoveAction(uid, psiAbility.Owner);
                }
            }

            _popups.PopupEntity(Loc.GetString("mindbreaking-feedback", ("entity", uid)),
                uid,
                // TODO: Use LoS-based Filter when one is available.
                Filter.Pvs(uid).RemoveWhereAttachedEntity(entity => !ExamineSystemShared.InRangeUnOccluded(uid, entity, ExamineRange, null)),
                true,
                PopupType.Medium);

            _statusEffectsSystem.TryAddStatusEffect(uid, "Stutter", TimeSpan.FromMinutes(5), false, "StutteringAccent");

            _glimmerSystem.Glimmer += _random.Next((int) MathF.Round(psionic.Amplification * psionic.Dampening * -5), (int) MathF.Round(psionic.Amplification * psionic.Dampening * -10));
            RemComp<PsionicComponent>(uid);
            RemComp<PotentialPsionicComponent>(uid);
        }
    }
}
