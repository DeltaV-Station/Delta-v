using System.Diagnostics.CodeAnalysis;
using Content.Shared.Body;
using Content.Shared.Chemistry.Reagent;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.Metabolism;

public sealed class MetabolizerWhitelistSystem : EntitySystem
{
    private EntityQuery<MetabolizerWhitelistComponent> _metabolizerWhitelistQuery;

    public override void Initialize()
    {
        base.Initialize();

        _metabolizerWhitelistQuery = GetEntityQuery<MetabolizerWhitelistComponent>();
    }

    /// <summary>
    /// Returns a set of metabolizable reagents, as per organs with MetabolizerWhitelistComponent.
    /// </summary>
    /// <param name="entity">Entity with BodyComponent</param>
    /// <param name="reagents">Set of reagents that may be metabolized by an organ.</param>
    /// <returns>True if any organ on this body has MetabolizerWhitelistComponent, false if otherwise.</returns>
    [PublicAPI]
    public bool GetMetabolizableReagents(Entity<BodyComponent> entity, [NotNullWhen(true)] out HashSet<ProtoId<ReagentPrototype>>? reagents)
    {
        var (_, body) = entity;

        if (body.Organs == null)
        {
            reagents = null;
            return false;
        }

        reagents = [];
        var foundMetabolizerWhitelist = false;
        foreach (var organ in body.Organs.ContainedEntities)
        {
            if (!_metabolizerWhitelistQuery.TryComp(organ, out var metabolizerWhitelist))
                continue;

            reagents.UnionWith(metabolizerWhitelist.ReagentWhitelist);
            foundMetabolizerWhitelist = true;
        }

        return foundMetabolizerWhitelist;
    }

    /// <summary>
    /// Checks if a target can metabolize a given reagent.
    /// </summary>
    /// <param name="entity">Entity with or without BodyComponent</param>
    /// <param name="reagent">The reagent to check for supporting metabolism</param>
    /// <returns></returns>
    [PublicAPI]
    public bool CanMetabolizeReagent(Entity<BodyComponent?> entity, ProtoId<ReagentPrototype> reagent)
    {
        var (uid, body) = entity;

        if (body is null && !Resolve(uid, ref body, false))
            return true;

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (!GetMetabolizableReagents((uid, body), out var reagents))
            return true;

        return reagents.Contains(reagent);
    }
}
