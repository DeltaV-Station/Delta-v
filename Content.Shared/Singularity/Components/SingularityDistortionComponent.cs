using Content.Shared.Singularity.EntitySystems;
using Content.Shared._DV.Singularity.EntitySystems; // DeltaV - Allow Noospherics to access this
using Robust.Shared.GameStates;

namespace Content.Shared.Singularity.Components
{
    [RegisterComponent, NetworkedComponent]
    [AutoGenerateComponentState]
    [Access(typeof(SharedSingularitySystem), typeof(SharedNoosphericSingularitySystem))] // DeltaV
    public sealed partial class SingularityDistortionComponent : Component
    {
        [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
        public float Intensity = 31.25f;

        [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
        public float FalloffPower = MathF.Sqrt(2f);
    }
}
