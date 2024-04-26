using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared.Patron
{
    /// <summary>
    /// Delta-V: A named supporter tier with priority (lower number = more important) and color for their name in OOC chat.
    /// </summary>
    public sealed class PatronTier
    {
        public string Name { get; }
        public int Priority { get; }
        public string Color { get; }

        public PatronTier(string name, int priority, string color)
        {
            Name = name;
            Priority = priority;
            Color = color;
        }
    }

}
