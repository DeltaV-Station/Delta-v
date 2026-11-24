using Content.Server.Ghost.Roles.Components;
using Content.Server.Speech.Components;
using Content.Server.StationEvents.Components;
using Content.Shared._DV.Cloning;
using Content.Shared._DV.Psionics.Components;
using Content.Shared.Damage.ForceSay;
using Content.Shared.Emoting;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mind.Components;
using Content.Shared.Preferences;
using Content.Shared.Speech;
using Content.Shared.SSDIndicator;
using Robust.Shared.GameObjects.Components.Localization;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server.Cloning;

public sealed partial class CloningSystem
{
    [Dependency] private readonly MetempsychoticMachineSystem _metem = default!;
    [Dependency] private readonly HumanoidProfileSystem _humanoidProfile = default!;
    [Dependency] private readonly GrammarSystem _grammar = default!;

    /// <summary>
    ///
    /// </summary>
    /// <param name="original">The original body EntityUId</param>
    /// <param name="humanoid">HumanoidProfile of the original body</param>
    /// <param name="coords">Coordinates of the Cloning Pod</param>
    /// <param name="metemComp">The MetempPsychoticMachineComponent of the Cloning Pod we're using </param>
    /// <param name="clone">The Resulting Clone or nothing</param>
    /// <returns></returns>
    private bool TryMetemClone(EntityUid original, HumanoidProfileComponent humanoid, MapCoordinates? coords, MetempsychoticMachineComponent? metemComp, out EntityUid? clone)
    {
        if (metemComp != null)
        {
            int? karma = null;
            if (TryComp<MetempsychoticKarmaComponent>(original, out var metempsychoticKarma))
            {
                karma = metempsychoticKarma.Karma;
            }

            var entityToSpawn = _metem.GetSpawnEntity((metemComp.Owner, metemComp),  out var newSpecies, karma);

            clone = coords == null ? Spawn(entityToSpawn) : Spawn(entityToSpawn, coords.Value);

            if (newSpecies != null)
            {
                var newProf = HumanoidCharacterProfile.RandomWithSpecies(newSpecies);
                _visualBody.ApplyProfileTo(clone.Value, newProf);
                _humanoidProfile.ApplyProfileTo(original, newProf);

                EnsureComp<MetempsychoticKarmaComponent>(clone.Value, out var karmaComp);
                karmaComp.Karma = karma != null ? karma.Value + 1 : 1;
            }
            else
            {
                var grammar = EnsureComp<GrammarComponent>(clone.Value);
                _grammar.SetProperNoun((clone.Value, grammar), true);
                _grammar.SetGender((clone.Value, grammar), humanoid.Gender);

                // Add missing components to players reincarnating into mobs
                EnsureComp<PotentialPsionicComponent>(clone.Value);
                EnsureComp<SpeechComponent>(clone.Value);
                EnsureComp<DamageForceSayComponent>(clone.Value);
                EnsureComp<EmotingComponent>(clone.Value);
                EnsureComp<MindContainerComponent>(clone.Value);
                EnsureComp<SSDIndicatorComponent>(clone.Value);
                RemComp<ReplacementAccentComponent>(clone.Value);
                RemComp<MonkeyAccentComponent>(clone.Value);
                RemComp<SentienceTargetComponent>(clone.Value);
                RemComp<GhostTakeoverAvailableComponent>(clone.Value);
            }

            return true;
        }

        clone = null;
        return false;
    }
}
