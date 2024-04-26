using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared.Patron
{

    /// <summary>
    /// Delta-V: A patron that supports the Delta-V project.
    /// </summary>
    public sealed class Patron
    {

        /// <summary>
        /// The "real" name of the patron, as reported by Patreon.
        /// </summary>
        public string RealName { get; }

        /// <summary>
        /// The in-game name of the patron, if specified in the Patrons.yml file.
        /// </summary>
        public string? UserName { get; internal set; }

        /// <summary>
        /// The effective name used to match a player.
        /// </summary>
        public string Name => UserName ?? RealName;

        public PatronTier Tier { get; }

        public Patron(string realName, PatronTier tier)
        {
            RealName = realName;
            Tier = tier;
        }
    }
}
