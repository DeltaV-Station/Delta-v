using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Roles;
using Content.Shared.Humanoid;

namespace Content.Server.GameTicking.Rules;

/// <summary>
/// Copy of ThiefRuleSystem
/// </summary>
public sealed class RoundstartFugitiveRuleSystem : GameRuleSystem<RoundstartFugitiveRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundstartFugitiveRuleComponent, AfterAntagEntitySelectedEvent>(AfterAntagSelected);

        SubscribeLocalEvent<RoundstartFugitiveRoleComponent, GetBriefingEvent>(OnGetBriefing);
    }

    // Greeting upon thief activation
    private void AfterAntagSelected(Entity<RoundstartFugitiveRuleComponent> mindId,
        ref AfterAntagEntitySelectedEvent args)
    {
        var ent = args.EntityUid;
        _antag.SendBriefing(ent, MakeBriefing(ent), null, null);
    }

    // Character screen briefing
    private void OnGetBriefing(Entity<RoundstartFugitiveRoleComponent> role, ref GetBriefingEvent args)
    {
        var ent = args.Mind.Comp.OwnedEntity;

        if (ent is null)
            return;
        args.Append(MakeBriefing(ent.Value));
    }

    private string MakeBriefing(EntityUid ent)
    {
        var isHuman = HasComp<HumanoidAppearanceComponent>(ent);
        var briefing = isHuman
            ? Loc.GetString("roundstartfugitive-role-greeting-human")
            : Loc.GetString("roundstartfugitive-role-greeting-animal"); //Can thieves be animals???

        if (isHuman)
            briefing += "\n \n" + Loc.GetString("roundstartfugitive-role-greeting-equipment") + "\n";

        return briefing;
    }
    // Copy of parts from FugitiveRules below here, hopefully this will cause the Fugitive Fax event to trigger?
}


