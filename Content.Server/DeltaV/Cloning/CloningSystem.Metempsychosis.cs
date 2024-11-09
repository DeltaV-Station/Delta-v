using Content.Server.DeltaV.Cloning;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Content.Shared.Speech;
using Content.Shared.Emoting;
using Content.Shared.Damage.ForceSay;
using Content.Shared.SSDIndicator;
using Content.Server.Speech.Components;
using Content.Server.Ghost.Roles.Components;
using Content.Server.StationEvents.Components;
using Content.Server.Psionics;
using Robust.Shared.Random;
using Content.Shared.Mind.Components;
using Content.Shared.Tag;
using Content.Shared.Cloning;
using Content.Shared.Random.Helpers;
using Robust.Shared.GameObjects.Components.Localization;

namespace Content.Server.Cloning;

public sealed partial class CloningSystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly GrammarSystem _grammar = default!;

    /// <summary>
    /// Gets the entity prototype to spawn for a clone based on karma and chance calculations.
    /// </summary>
    private string GetSpawnEntity(Entity<MetempsychoticMachineComponent> ent, float karmaBonus, SpeciesPrototype oldSpecies, out SpeciesPrototype? species, int karma = 0)
    {
        // First time being cloned - return original species
        if (karma == 0)
        {
            species = oldSpecies;
            return oldSpecies.Prototype;
        }

        var chance = ent.Comp.HumanoidBaseChance + karmaBonus;
        chance -= (1 - ent.Comp.HumanoidBaseChance) * karma;

        // Perfect clone chance
        if (chance > 1 && _robustRandom.Prob(chance - 1))
        {
            species = oldSpecies;
            return oldSpecies.Prototype;
        }

        // Roll for humanoid vs non-humanoid
        chance = Math.Clamp(chance, 0, 1);
        if (_robustRandom.Prob(chance))
        {
            if (_prototype.TryIndex(ent.Comp.MetempsychoticHumanoidPool, out var humanoidPool))
            {
                var protoId = humanoidPool.Pick();
                if (_prototype.TryIndex<SpeciesPrototype>(protoId, out var speciesPrototype))
                {
                    species = speciesPrototype;
                    return speciesPrototype.Prototype;
                }
            }
        }
        else if (_prototype.TryIndex(ent.Comp.MetempsychoticNonHumanoidPool, out var nonHumanoidPool))
        {
            // For non-humanoids, return the entity prototype directly
            species = null;
            return nonHumanoidPool.Pick();
        }

        // Fallback to original species if prototype indexing fails
        Log.Error("Failed to get valid clone type - falling back to original species");
        species = oldSpecies;
        return oldSpecies.Prototype;
    }

    /// <summary>
    /// Handles fetching the mob and managing appearance for cloning with metempsychosis mechanics
    /// </summary>
    private EntityUid FetchAndSpawnMob(
        Entity<CloningPodComponent> pod,
        HumanoidCharacterProfile pref,
        SpeciesPrototype speciesPrototype,
        HumanoidAppearanceComponent humanoid,
        EntityUid bodyToClone,
        float karmaBonus)
    {
        List<Sex> sexes = [];
        var switchingSpecies = false;
        var applyKarma = false;
        var toSpawn = speciesPrototype.Prototype;

        // Get existing karma score or start at 0
        var karmaScore = 0;
        if (TryComp<MetempsychosisKarmaComponent>(bodyToClone, out var oldKarma))
        {
            karmaScore = oldKarma.Score;
        }

        if (TryComp<MetempsychoticMachineComponent>(pod.Owner, out var metem))
        {
            var metemEntity = new Entity<MetempsychoticMachineComponent>(pod.Owner, metem);
            toSpawn = GetSpawnEntity(metemEntity, karmaBonus, speciesPrototype, out var newSpecies, karmaScore);
            applyKarma = true;

            if (newSpecies != null)
            {
                sexes = newSpecies.Sexes;
                speciesPrototype = newSpecies;

                if (speciesPrototype.ID != newSpecies.ID)
                    switchingSpecies = true;
            }
        }

        var mob = Spawn(toSpawn, _transformSystem.GetMapCoordinates(pod.Owner));

        // Only try to handle humanoid appearance if we have a humanoid component
        if (TryComp<HumanoidAppearanceComponent>(mob, out var newHumanoid))
        {
            if (switchingSpecies || HasComp<MetempsychosisKarmaComponent>(bodyToClone))
            {
                pref = HumanoidCharacterProfile.RandomWithSpecies(newHumanoid.Species);
                if (sexes.Contains(humanoid.Sex))
                    pref = pref.WithSex(humanoid.Sex);

                pref = pref.WithGender(humanoid.Gender);
                pref = pref.WithAge(humanoid.Age);
            }

            _humanoidSystem.LoadProfile(mob, pref);
        }

        if (applyKarma)
        {
            var karma = EnsureComp<MetempsychosisKarmaComponent>(mob);
            karma.Score = karmaScore + 1; // Increment karma score
        }

        var ev = new CloningEvent(bodyToClone, mob);
        RaiseLocalEvent(bodyToClone, ref ev);

        if (!ev.NameHandled)
            _metaSystem.SetEntityName(mob, MetaData(bodyToClone).EntityName);

        var grammar = EnsureComp<GrammarComponent>(mob);
        var grammarEnt = new Entity<GrammarComponent>(mob, grammar);
        _grammar.SetProperNoun(grammarEnt, true);
        _grammar.SetGender(grammarEnt, humanoid.Gender);
        Dirty(mob, grammar);

        SetupBasicComponents(mob);

        return mob;
    }

    // I hate this
    private void SetupBasicComponents(EntityUid mob)
    {
        EnsureComp<PotentialPsionicComponent>(mob);
        EnsureComp<SpeechComponent>(mob);
        EnsureComp<DamageForceSayComponent>(mob);
        EnsureComp<EmotingComponent>(mob);
        EnsureComp<MindContainerComponent>(mob);
        EnsureComp<SSDIndicatorComponent>(mob);
        RemComp<ReplacementAccentComponent>(mob);
        RemComp<MonkeyAccentComponent>(mob);
        RemComp<SentienceTargetComponent>(mob);
        RemComp<GhostTakeoverAvailableComponent>(mob);

        _tag.AddTag(mob, "DoorBumpOpener");
    }
}
