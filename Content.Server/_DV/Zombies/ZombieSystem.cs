using Content.Server._DV.Psionics.Systems;
using Content.Server.Antag;
using Content.Shared._DV.Movement.Components;
using Content.Shared._DV.Psionics.Components;
using Content.Shared.CharacterInfo;
using Content.Shared.Cloning.Events;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.NPC.Components;
using Content.Shared.Radio.Components;
using Content.Shared.Roles.Components;
using Content.Shared.Zombies;
using Robust.Shared.Prototypes;

namespace Content.Server.Zombies;

public sealed partial class ZombieSystem
{
    //private static readonly string MindRoleInitialInfected = "MindRoleInitialInfected";
    private static readonly EntProtoId InitialInfectedFailureSurviveObjective = "InitialInfectedFailureSurviveObjective";

    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly PsionicSystem _psionic = default!; // DeltaV
    [Dependency] private readonly SharedJetpackSystem _jetpack = default!; // DeltaV - Prevent Jetpacks on Zombies

    private void InitializeDV()
    {
        // This needs to be done before CloneEvent or else the proper attributes won't be restored or modified on
        // the new entity.
        SubscribeLocalEvent<ZombieComponent, CloningAttemptEvent>(OnBeforeUnzombify);
        SubscribeLocalEvent<InitialInfectedComponent, CloningEvent>(OnInitialInfectedCloning);
    }

    /// <summary>
    /// DeltaV - Extra things we want to ensure, remove, or take care of when someone is zombified.
    /// </summary>
    private void ZombifyEntityDV(Entity<ZombieComponent> ent, MobStateComponent? mobState = null)
    {
        // Remove innate radio
        RemComp<ActiveRadioComponent>(ent); // If the zombie has an innate radio, get rid of it.

        // Remove headsets in pockets
        for (var i = 1; i <= 4; i++) // Arachnids have 4 pockets
        {
            if (_inventory.TryGetSlotEntity(ent, $"pocket{i}", out var headset) && HasComp<HeadsetComponent>(headset))
                _inventory.TryUnequip(ent, $"pocket{i}", true, true);
        }

        // Prevent Psionic Zombies
        RemComp<PotentialPsionicComponent>(ent);
        _psionic.MindBreakEntity(ent.Owner, false, true);

        // Prevent Jetpacks on Zombies
        if (TryComp<JetpackUserComponent>(ent, out var jetpackUser))
        {
            if (TryComp<JetpackComponent>(jetpackUser.Jetpack, out var jetpack))
                _jetpack.SetEnabled(jetpackUser.Jetpack, jetpack, false, ent);
        }
        RemComp<AutomaticJetpackUserComponent>(ent);

        // Prevent shitters biting other zombies
        EnsureComp<NoFriendlyFireComponent>(ent);
    }

    /// <summary>
    /// DeltaV - We need to save some extra things to the ZombieComponent so we can restore them when
    /// they are about to be cloned.
    /// </summary>
    /// <param name="ent"></param>
    private void PreserveEntityComponentState(Entity<ZombieComponent> ent)
    {
        SaveFactionsBeforeZombification(ent);

        // Zombification removes pacifism but lets track if they have it so the pacifist players will be... pacified.
        ent.Comp.WasPacisfistBeforeZombification = HasComp<PacifiedComponent>(ent);
    }

    private void SaveFactionsBeforeZombification(Entity<ZombieComponent> ent)
    {
        if (!TryComp<NpcFactionMemberComponent>(ent, out var faction))
            return;

        ent.Comp.FactionsBeforeZombification.AddRange(faction.Factions);
    }


    /// <summary>
    /// DeltaV - Restores certain components to their previous state that the upstream unzombify doesn't handle.
    /// We need to do this before CloningEvent so the cloned body will have the updated components BEFORE cloning happens.
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="args"></param>
    private void OnBeforeUnzombify(Entity<ZombieComponent> ent, ref CloningAttemptEvent args)
    {
        // Restore factions
        _faction.ClearFactions(ent.Owner); // Should only be zombie, but might as well clear the whole list in case.
        ent.Comp.FactionsBeforeZombification.ForEach(previousFaction => _faction.AddFaction(ent.Owner, previousFaction));

        // Restore pacifism
        if (ent.Comp.WasPacisfistBeforeZombification)
            EnsureComp<PacifiedComponent>(ent);
    }

    /// <summary>
    /// DeltaV - Adds a survival objective to Initial Infected who are cloned. Chances are that the crew has recovered, so
    /// bioterrorism time is probably over.
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="args"></param>
    private void OnInitialInfectedCloning(Entity<InitialInfectedComponent> ent, ref CloningEvent args)
    {
        // Change II objectives so they no longer have infect objectives. Now, they'll have survival objectives. Live to infect another day. :)
        _mind.TryGetMind(ent.Owner, out var mindId, out var mindContainer);

        // If we can't find the mind, oh well. We tried.
        if (mindContainer is not { } mind)
            return;

        if (_role.MindHasRole<InitialInfectedRoleComponent>(mindId))
            _mind.TryAddObjective(mindId, mind, InitialInfectedFailureSurviveObjective);
    }
}
