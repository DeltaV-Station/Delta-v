using Content.Shared.Kitchen.Components;

namespace Content.Server.Kitchen.Components
{
    [RegisterComponent]
    [ComponentReference(typeof(SharedDeepFriedComponent))]
    public sealed class DeepFriedComponent : SharedDeepFriedComponent
    {
        /// <summary>
        /// What is the item's base price multiplied by?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("priceCoefficient")]
        public float PriceCoefficient { get; set; } = 1.0f;

        /// <summary>
        /// What was the entity's original name before any modification?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("originalName")]
        public string? OriginalName { get; set; }
    }
}
