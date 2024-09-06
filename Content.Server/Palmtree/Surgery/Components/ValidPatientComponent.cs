using System.Collections.Generic;

namespace Content.Server.Palmtree.Surgery
{
    [RegisterComponent]
    public partial class PPatientComponent : Component //"PPatient" because wizden might add surgery down the line, so I'm doing this to avoid conflicts.
    {
        public List<string> procedures = new List<string>();
    }
}
