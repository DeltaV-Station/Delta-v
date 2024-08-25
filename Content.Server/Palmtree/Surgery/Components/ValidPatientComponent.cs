using System.Collections.Generic;

namespace Content.Server.Palmtree.Surgery
{
    [RegisterComponent]
    public partial class PPatientComponent : Component //"PPatient" because wizden might add surgery down the line, so I'm doing this to avoid conflicts.
    {// I'll make this better later with a proper list of steps, I just need a first version for now
        [DataField("incised")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool incised = false;

        [DataField("retracted")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool retracted = false;

        [DataField("clamped")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool clamped = false;

        public List<string> procedures = new List<string>();
    }
}
