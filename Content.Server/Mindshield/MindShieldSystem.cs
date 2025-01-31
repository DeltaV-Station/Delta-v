using Content.Server.Administration.Logs;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Shared.Database;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Mindshield.Components;
using Content.Shared.NPC.Components; // DeltaV
using Content.Shared.Revolutionary.Components;
using Content.Shared.Tag;

namespace Content.Server.Mindshield;

/// <summary>
/// System used for checking if the implanted is a Rev or Head Rev.
/// </summary>
public sealed class MindShieldSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    [ValidatePrototypeId<TagPrototype>]
    public const string MindShieldTag = "MindShield";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SubdermalImplantComponent, ImplantImplantedEvent>(OnImplanted); // DeltaV: separate handlers for implanting and removal
        SubscribeLocalEvent<SubdermalImplantComponent, ImplantRemovedEvent>(OnRemoved); // DeltaV
    }

    /// <summary>
    /// DeltaV: Adds components when implantedChecks if the implant was a mindshield or not
    /// </summary>
    public void OnImplanted(EntityUid uid, SubdermalImplantComponent comp, ref ImplantImplantedEvent ev)
    {
        if (ev.Implanted is not {} user)
            return;

        if (comp.AddedComponents is { } components)
        {
            EntityManager.AddComponents(user, components);
            if (components.ContainsKey("MindShield"))
            {
                MindShieldRemovalCheck(ev.Implanted.Value, ev.Implant);
            }

        }
    }

    /// <summary>
    /// DeltaV: Removes components when implanted.
    /// </summary>
    private void OnRemoved(Entity<SubdermalImplantComponent> ent, ref ImplantRemovedEvent args)
    {
        if (ent.Comp.AddedComponents is {} components && args.Implanted is {} user)
            EntityManager.RemoveComponents(user, components);
    }

    /// <summary>
    /// Checks if the implanted person was a Rev or Head Rev and remove role or destroy mindshield respectively.
    /// DeltaV: Destroys mindshield on Syndicates as well.
    /// </summary>
    public void MindShieldRemovalCheck(EntityUid implanted, EntityUid implant)
    {
        // DeltaV - Destroy Mindshield on Syndicates
        if (TryComp<NpcFactionMemberComponent>(implanted, out var component))
        {
            var factions = component.Factions;
            if (factions.Contains("Syndicate"))
            {
                QueueDel(implant);
                // No popup as to allow more sneaky opportunities for syndicates.
            }
        }
        // DeltaV - End destroy Mindshield on Syndicates

        if (HasComp<HeadRevolutionaryComponent>(implanted))
        {
            _popupSystem.PopupEntity(Loc.GetString("head-rev-break-mindshield"), implanted);
            QueueDel(implant);
            return;
        }

        if (_mindSystem.TryGetMind(implanted, out var mindId, out _) &&
            _roleSystem.MindTryRemoveRole<RevolutionaryRoleComponent>(mindId))
        {
            _adminLogManager.Add(LogType.Mind, LogImpact.Medium, $"{ToPrettyString(implanted)} was deconverted due to being implanted with a Mindshield.");
        }
    }
}
