using Content.Server.GameTicking.Rules;
using Content.Server.Roles;
using Content.Shared._DV.Roles;

namespace Content.Server._DV.Uprising;

public sealed class UprisingRuleSystem : GameRuleSystem<UprisingRuleComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LoyalistRoleComponent, GetBriefingEvent>(OnGetLoyalistBriefing);
        SubscribeLocalEvent<InsurgentRoleComponent, GetBriefingEvent>(OnGetInsurgentBriefing);
    }

    private void OnGetLoyalistBriefing(Entity<LoyalistRoleComponent> ent, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("antag-Loyalist.briefing"));
    }

    private void OnGetInsurgentBriefing(Entity<InsurgentRoleComponent> ent, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("antag-Insurgent.briefing"));
    }
}
