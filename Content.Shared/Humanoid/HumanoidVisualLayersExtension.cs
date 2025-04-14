using Content.Shared.Body.Components;
using Content.Shared.Body.Part;

namespace Content.Shared.Humanoid
{
    public static class HumanoidVisualLayersExtension
    {
        public static bool HasSexMorph(HumanoidVisualLayers layer)
        {
            return layer switch
            {
                HumanoidVisualLayers.Chest => true,
                HumanoidVisualLayers.Head => true,
                _ => false
            };
        }

        public static string GetSexMorph(HumanoidVisualLayers layer, Sex sex, string id)
        {
            if (!HasSexMorph(layer) || sex == Sex.Unsexed)
                return id;

            return $"{id}{sex}";
        }

        /// <summary>
        ///     Sublayers. Any other layers that may visually depend on this layer existing.
        ///     For example, the head has layers such as eyes, hair, etc. depending on it.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns>Enumerable of layers that depend on that given layer. Empty, otherwise.</returns>
        /// <remarks>This could eventually be replaced by a body system implementation.</remarks>
        public static IEnumerable<HumanoidVisualLayers> Sublayers(HumanoidVisualLayers layer)
        {
            switch (layer)
            {
                case HumanoidVisualLayers.Head:
                    yield return HumanoidVisualLayers.Head;
                    yield return HumanoidVisualLayers.Eyes;
                    yield return HumanoidVisualLayers.HeadSide;
                    yield return HumanoidVisualLayers.HeadTop;
                    yield return HumanoidVisualLayers.Hair;
                    yield return HumanoidVisualLayers.FacialHair;
                    yield return HumanoidVisualLayers.Snout;
                    break;
                case HumanoidVisualLayers.LArm:
                    yield return HumanoidVisualLayers.LArm;
                    yield return HumanoidVisualLayers.LHand;
                    break;
                case HumanoidVisualLayers.RArm:
                    yield return HumanoidVisualLayers.RArm;
                    yield return HumanoidVisualLayers.RHand;
                    break;
                case HumanoidVisualLayers.LLeg:
                    yield return HumanoidVisualLayers.LLeg;
                    yield return HumanoidVisualLayers.LFoot;
                    break;
                case HumanoidVisualLayers.RLeg:
                    yield return HumanoidVisualLayers.RLeg;
                    yield return HumanoidVisualLayers.RFoot;
                    break;
                case HumanoidVisualLayers.Chest:
                    yield return HumanoidVisualLayers.Chest;
                    yield return HumanoidVisualLayers.Tail;
                    break;
                // Shitmed Change Start
                case HumanoidVisualLayers.LHand:
                    yield return HumanoidVisualLayers.LHand;
                    break;
                case HumanoidVisualLayers.RHand:
                    yield return HumanoidVisualLayers.RHand;
                    break;
                case HumanoidVisualLayers.LFoot:
                    yield return HumanoidVisualLayers.LFoot;
                    break;
                case HumanoidVisualLayers.RFoot:
                    yield return HumanoidVisualLayers.RFoot;
                    break;
                // Shitmed Change End
                default:
                    yield break;
            }
        }

        // Begin DeltaV - We want this data w/o needing a BodyPartComponnet
        public static HumanoidVisualLayers? ToHumanoidLayers(this BodyPartComponent part)
        {
            return part.PartType.ToHumanoidLayers(part.Symmetry);
        }

        public static HumanoidVisualLayers? ToHumanoidLayers(this BodyPartType partType, BodyPartSymmetry partSymmetry)
        {
            switch (partType)
            {
                case BodyPartType.Other:
                    break;
                case BodyPartType.Torso:
                    return HumanoidVisualLayers.Chest;
                case BodyPartType.Tail:
                    return HumanoidVisualLayers.Tail;
                case BodyPartType.Head:
                    // use the Sublayers method to hide the rest of the parts,
                    // if that's what you're looking for
                    return HumanoidVisualLayers.Head;
                case BodyPartType.Arm:
                    switch (partSymmetry)
                    {
                        case BodyPartSymmetry.None:
                            break;
                        case BodyPartSymmetry.Left:
                            return HumanoidVisualLayers.LArm;
                        case BodyPartSymmetry.Right:
                            return HumanoidVisualLayers.RArm;
                    }

                    break;
                case BodyPartType.Hand:
                    switch (partSymmetry)
                    {
                        case BodyPartSymmetry.None:
                            break;
                        case BodyPartSymmetry.Left:
                            return HumanoidVisualLayers.LHand;
                        case BodyPartSymmetry.Right:
                            return HumanoidVisualLayers.RHand;
                    }

                    break;
                case BodyPartType.Leg:
                    switch (partSymmetry)
                    {
                        case BodyPartSymmetry.None:
                            break;
                        case BodyPartSymmetry.Left:
                            return HumanoidVisualLayers.LLeg;
                        case BodyPartSymmetry.Right:
                            return HumanoidVisualLayers.RLeg;
                    }

                    break;
                case BodyPartType.Foot:
                    switch (partSymmetry)
                    {
                        case BodyPartSymmetry.None:
                            break;
                        case BodyPartSymmetry.Left:
                            return HumanoidVisualLayers.LFoot;
                        case BodyPartSymmetry.Right:
                            return HumanoidVisualLayers.RFoot;
                    }

                    break;
            }

            return null;
        }
        // End DeltaV - We want this data w/o needing a BodyPartComponnet
    }
}
