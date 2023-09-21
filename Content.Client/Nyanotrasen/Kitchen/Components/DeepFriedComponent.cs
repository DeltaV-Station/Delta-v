using Content.Shared.Kitchen.Components;

namespace Content.Client.Kitchen.Components
{
    [RegisterComponent]
    [ComponentReference(typeof(SharedDeepFriedComponent))]
    public sealed class DeepFriedComponent : SharedDeepFriedComponent
    {
    }
}
