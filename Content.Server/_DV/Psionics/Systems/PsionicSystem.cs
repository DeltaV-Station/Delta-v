using System.Linq;
using Content.Server._DV.Psionics.UI;
using Content.Server.EUI;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Systems;
using Content.Shared.EntityTable;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._DV.Psionics.Systems;

public sealed class PsionicSystem : SharedPsionicSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PotentialPsionicComponent, PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnPlayerSpawnComplete(Entity<PotentialPsionicComponent> potPsionic, ref PlayerSpawnCompleteEvent args)
    {
        var chance = potPsionic.Comp.BaseChance;
        // Jobs like Command and Chaplains get a bonus on their roll.
        chance += potPsionic.Comp.JobBonusChance;
        // Species like Kitsunes get a bonus on their roll.
        chance += potPsionic.Comp.SpeciesBonusChance;

        chance = Math.Clamp(chance, 0, 1);

        if (Random.Prob(chance))
            _euiManager.OpenEui(new AcceptPsionicsEui(potPsionic, this), args.Player);

    }

    public void AddRandomPsionicPower(Entity<PotentialPsionicComponent> psionic, int attempts = 0)
    {
        var spawns = _entityTable.GetSpawns(psionic.Comp.AvailablePsionics);

        foreach (var entProtoId in spawns)
        {
            if (!_prototypeManager.Resolve(entProtoId, out var psionicComponents))
                continue;
            // If the psionic already has that power, do not add it again and retry again.
            if (psionicComponents.Components.Any(psionicComponent =>
                    EntityManager.HasComponent(psionic, psionicComponent.Value.Component.GetType())))
                continue;
            // If they don't have it already, add it and break out.
            EntityManager.AddComponents(psionic, psionicComponents);
            return;
        }

        attempts++;
        if (attempts > 20) // 20 attempts to get a unique psionic power.
            AddRandomPsionicPower(psionic, attempts);
    }
}
