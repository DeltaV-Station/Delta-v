using Content.Shared.Kitchen.Components;

namespace Content.Client.Kitchen.Components
{
    [RegisterComponent]
    [ComponentReference(typeof(SharedDeepFryerComponent))]
    public sealed class DeepFryerComponent : SharedDeepFryerComponent
    {
    }
}
