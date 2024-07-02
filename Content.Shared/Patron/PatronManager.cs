using Robust.Shared.ContentPack;
using Robust.Shared.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Content.Shared.Patron;

/// <summary>
/// Delta-V: Centralized utility for loading Patrons and finding the corresponding Tier properties.
/// Shared because the Server needs it for OOC chat, and Client needs it for the Credits tab.
/// </summary>
public sealed class PatronManager
{

    /// <summary>
    /// Contains all PatronTiers, indexed by name.
    /// </summary>
    public IDictionary<string, PatronTier>? PatronTiers { get; private set; }

    /// <summary>
    /// Contains all Patrons, indexed by names that can be matched.
    /// </summary>
    public IEnumerable<Patron>? Patrons { get; private set; }

    public void Load(IResourceManager resourceManager)
    {
        LoadTiers(resourceManager);
        LoadPatrons(resourceManager);
        LoadPatronUsernames(resourceManager);
    }

    /// <summary>
    /// Parses the PatronTiers.yml file.
    /// </summary>
    private void LoadTiers(IResourceManager resourceManager)
    {
        var yamlStream = resourceManager.ContentFileReadYaml(new("/PatronTiers.yml"));
        var sequence = (YamlSequenceNode) yamlStream.Documents[0].RootNode;

        PatronTiers = sequence
            .Cast<YamlMappingNode>()
            .Select(m => new PatronTier(m["Name"].AsString(), m["Priority"].AsInt(), m["Color"].AsString()))
            .ToDictionary(patron => patron.Name);
    }

    /// <summary>
    /// Parses the Patrons.yml file.
    /// </summary>
    private void LoadPatrons(IResourceManager resourceManager)
    {
        if (PatronTiers == null)
        {
            throw new NullReferenceException();
        }

        var yamlStream = resourceManager.ContentFileReadYaml(new("/Patrons.yml"));
        var sequence = (YamlSequenceNode) yamlStream.Documents[0].RootNode;

        Patrons = sequence
            .Cast<YamlMappingNode>()
            .Select(m => new Patron(m["Name"].AsString(), PatronTiers[m["Tier"].AsString()]))
            .ToList();
    }

    /// <summary>
    /// Parses the PatronUsernames.yml, containing string-to-string mappings from Patreon name to Username.
    /// The username takes precedence for matching Patrons.
    /// </summary>
    /// <param name="resourceManager"></param>
    private void LoadPatronUsernames(IResourceManager resourceManager)
    { 
        var yamlStream = resourceManager.ContentFileReadYaml(new("/PatronUsernames.yml"));
        var sequence = (YamlSequenceNode) yamlStream.Documents[0].RootNode;

        foreach (var patronMapping in sequence
            .Cast<YamlMappingNode>()
            .Select(m => new { PatronName = m["Patron"].AsString(), UserName = m["Username"].AsString() }))
        {
            // If a patron exists with the given Patron name, we assign it the Username.
            // After this assignment, the Patron will only be findable by Username, as that takes precedence.
            var patron = GetPatronByName(patronMapping.PatronName);
            if (patron != null)
            {
                patron.UserName = patronMapping.UserName;
            }
        }
    }

    /// <summary>
    /// Find the Patron object for a given player name.
    /// </summary>
    public Patron? GetPatronByName(string playerName)
    {
        return Patrons?
            .Where(p => p.Name.ToLower() == playerName.ToLower())
            .FirstOrDefault(null as Patron);
    }

}

