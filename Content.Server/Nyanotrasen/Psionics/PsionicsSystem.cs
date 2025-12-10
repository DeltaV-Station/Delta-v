using Content.Shared.StatusEffect;
using Content.Shared.Psionics.Glimmer;
using Content.Server.Abilities.Psionics;
using Content.Server.Chat.Systems;
using Content.Server.Electrocution;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Random;
using Content.Shared._DV.Psionics.Components;

namespace Content.Server.Psionics
{
    public sealed class PsionicsSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly PsionicAbilitiesSystem _psionicAbilitiesSystem = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
        [Dependency] private readonly ElectrocutionSystem _electrocutionSystem = default!;
        [Dependency] private readonly MindSwapPowerSystem _mindSwapPowerSystem = default!;
        [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly NpcFactionSystem _faction = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PsionicComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<PsionicComponent, ComponentRemove>(OnRemove);
        }

        private void OnInit(EntityUid uid, PsionicComponent component, ComponentInit args)
        {
            if (!component.Removable)
                return;

            if (!TryComp<NpcFactionMemberComponent>(uid, out var factions))
                return;

            if (_faction.IsMember((uid, factions), "GlimmerMonster"))
                return;

            _faction.AddFaction((uid, factions), "PsionicInterloper");
        }

        private void OnRemove(EntityUid uid, PsionicComponent component, ComponentRemove args)
        {
            if (!TryComp<NpcFactionMemberComponent>(uid, out var factions))
                return;

            _faction.RemoveFaction((uid, factions), "PsionicInterloper");
        }



        // /// <summary>
        // /// Makes the entity psionic if it is possible.
        // /// Ignores rolling and rerolling prevention.
        // /// </summary>
        // public bool TryMakePsionic(Entity<OldPotentialPsionicComponent> ent)
        // {
        //     if (HasComp<PsionicComponent>(ent))
        //         return false;
        //
        //     if (!_cfg.GetCVar(DCCVars.PsionicRollsEnabled))
        //         return false;
        //
        //     var warn = CompOrNull<PsionicBonusChanceComponent>(ent)?.Warn ?? true;
        //     _psionicAbilitiesSystem.AddPsionics(ent, warn);
        //     return true;
        // }

        // public void RollPsionics(EntityUid uid, OldPotentialPsionicComponent component, bool applyGlimmer = true, float multiplier = 1f)
        // {
        //
        //     var chance = component.Chance;
        //     if (TryComp<PsionicBonusChanceComponent>(uid, out var bonus))
        //     {
        //         chance *= bonus.Multiplier;
        //         chance += bonus.FlatBonus;
        //     }
        //
        //     if (applyGlimmer)
        //         chance += ((float) _glimmerSystem.Glimmer / 1000);
        //
        //     chance *= multiplier;
        //
        //     chance = Math.Clamp(chance, 0, 1);
        //
        //     if (_random.Prob(chance))
        //         TryMakePsionic((uid, component));
        // }

        // public void RerollPsionics(EntityUid uid, OldPotentialPsionicComponent? psionic = null, float bonusMuliplier = 1f)
        // {
        //     if (!Resolve(uid, ref psionic, false))
        //         return;
        //
        //     if (psionic.Rerolled)
        //         return;
        //
        //     // RollPsionics(uid, psionic, multiplier: bonusMuliplier);
        //     psionic.Rerolled = true;
        // }
        //
        // public void GrantNewPsionicReroll(EntityUid uid, OldPotentialPsionicComponent? psionic = null)
        // {
        //     if (!Resolve(uid, ref psionic, false))
        //         return;
        //
        //     psionic.Rerolled = false;
        // }
    }
}
