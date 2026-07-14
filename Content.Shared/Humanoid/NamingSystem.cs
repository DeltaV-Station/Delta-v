using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Dataset;
using Content.Shared.Random.Helpers;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Enums;

namespace Content.Shared.Humanoid
{
    /// <summary>
    /// Figure out how to name a humanoid with these extensions.
    /// </summary>
    public sealed class NamingSystem : EntitySystem
    {
        private static readonly ProtoId<SpeciesPrototype> FallbackSpecies = "Human";

        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        // DeltaV - i hate this hack but https://github.com/space-wizards/RobustToolbox/issues/6576
        private ISawmill _locSawmill = default!;

        public override void Initialize()
        {
            base.Initialize();

            _locSawmill = Logger.GetSawmill("loc");
        }
        // End DeltaV - i hate this hack

        public string GetName(string species, Gender? gender = null)
        {
            // if they have an old species or whatever just fall back to human I guess?
            // Some downstream is probably gonna have this eventually but then they can deal with fallbacks.
            if (!_prototypeManager.TryIndex(species, out SpeciesPrototype? speciesProto))
            {
                speciesProto = _prototypeManager.Index(FallbackSpecies);
                Log.Warning($"Unable to find species {species} for name, falling back to {FallbackSpecies}");
            }

            switch (speciesProto.Naming)
            {
                case SpeciesNaming.First:
                    return Loc.GetString("namepreset-first",
                        ("first", GetFirstName(speciesProto, gender)));
                // Start of Nyano - Summary: for Oni naming
                case SpeciesNaming.LastNoFirst:
                    return Loc.GetString("namepreset-lastnofirst",
                        ("first", GetFirstName(speciesProto, gender)), ("last", GetLastName(speciesProto)));
                // End of Nyano - Summary: for Oni naming
                case SpeciesNaming.TheFirstofLast:
                    return Loc.GetString("namepreset-thefirstoflast",
                        ("first", GetFirstName(speciesProto, gender)), ("last", GetLastName(speciesProto)));
                case SpeciesNaming.FirstDashFirst:
                    return Loc.GetString("namepreset-firstdashfirst",
                        ("first1", GetFirstName(speciesProto, gender)), ("first2", GetFirstName(speciesProto, gender)));
                case SpeciesNaming.LastFirst: // DeltaV: Rodentia name scheme
                    return Loc.GetString("namepreset-lastfirst",
                        ("last", GetLastName(speciesProto)), ("first", GetFirstName(speciesProto, gender)));
                case SpeciesNaming.FirstLast:
                // Begin DeltaV - more complex naming
                default:
                {
                    var firstId = GetFirstNameId(speciesProto, gender);
                    var lastId = GetLastNameId(speciesProto);
                    var plural = false;

                    var oldLevel =_locSawmill.Level;
                    _locSawmill.Level = LogLevel.Fatal; // this is a hack to avoid testfails because TryGetString still logs errors for not-found stuff anyways (wtf)
                    if (Loc.TryGetString($"{lastId}.plural", out var pluralStr))
                        plural = pluralStr == "true";

                    var last = Loc.GetString(lastId);

                    if (Loc.TryGetString($"{firstId}.intersperse", out var firstIntersperse, ("last", last), ("lastPlural", plural)))
                    {
                        _locSawmill.Level = oldLevel;
                        return firstIntersperse;
                    }

                    _locSawmill.Level = oldLevel;
                    return Loc.GetString("namepreset-firstlast",
                        ("first", Loc.GetString(firstId)),
                        ("last", last),
                        ("lastPlural", plural));
                }
                // End DeltaV - more complex naming
            }
        }

        // Begin DeltaV - we want IDs
        public string GetFirstName(SpeciesPrototype speciesProto, Gender? gender = null)
        {
            return Loc.GetString(GetFirstNameId(speciesProto, gender));
        }

        public string GetFirstNameId(SpeciesPrototype speciesProto, Gender? gender = null)
        {
            switch (gender)
            {
                case Gender.Male:
                    return _random.PickId(_prototypeManager.Index(speciesProto.MaleFirstNames));
                case Gender.Female:
                    return _random.PickId(_prototypeManager.Index(speciesProto.FemaleFirstNames));
                default:
                    if (_random.Prob(0.5f))
                        return _random.PickId(_prototypeManager.Index(speciesProto.MaleFirstNames));
                    else
                        return _random.PickId(_prototypeManager.Index(speciesProto.FemaleFirstNames));
            }
        }

        public string GetLastNameId(SpeciesPrototype speciesProto)
        {
            return _random.PickId(_prototypeManager.Index(speciesProto.LastNames));
        }

        public string GetLastName(SpeciesPrototype speciesProto)
        {
            return Loc.GetString(GetLastNameId(speciesProto));
        }
        // End DeltaV - we want IDs
    }
}
