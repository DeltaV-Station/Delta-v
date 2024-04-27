using System.Diagnostics.CodeAnalysis;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences.Loadouts.Effects;

/// <summary>
/// Denies certain species from wearing a loadout. Prevents choosing by non-humanoids.
/// </summary>
public sealed partial class SpeciesRequirementLoadoutEffect : LoadoutEffect
{
    [DataField(required: true)]
    public List<ProtoId<SpeciesPrototype>> Deny = [];

    public override bool Validate(ICharacterProfile? profile, RoleLoadout loadout, ICommonSession? session,
        IDependencyCollection collection,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        if (profile is not HumanoidCharacterProfile humanoidCharacterProfile)
        {
            // TODO: localize
            reason = FormattedMessage.FromUnformatted("You are not humanoid.");
            return false;
        }

        if (Deny.Contains(humanoidCharacterProfile.Species))
        {
            // TODO: localize
            reason = FormattedMessage.FromUnformatted("Your species cannot equip this.");
            return false;
        }

        reason = null;
        return true;
    }
}
