using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared.Patron;

/// <summary>
/// Delta-V: A named supporter tier with priority (lower number = more important) and color for their name in OOC chat.
/// </summary>
public sealed class PatronTier
{
    /// <summary>
    /// The name of this tier. This should match the values in Patrons.yml.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The priority of this tier when it comes to visual rendering. A lower amount means higher on the list of tiers.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// The OOC chat username color that this tier grants, as hex code, e.g.: "#00ff00".
    /// </summary>
    public string Color { get; }

    public PatronTier(string name, int priority, string color)
    {
        Name = name;
        Priority = priority;
        Color = color;
    }
}
