using Content.Server.GameTicking.Rules.Components;
using Content.Server.Roles;
using Content.Shared.Humanoid;
using Content.Server.Roles;


namespace Content.Server._DV.NTAgent;

using Content.Server.GameTicking.Rules;
public sealed class NTAgentRuleSystem : GameRuleSystem<NTAgentRuleComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NTAgentRuleComponent, GetBriefingEvent>(OnGetBriefing);
    }

    private void OnGetBriefing(Entity<NTAgentRuleComponent> ent, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("NTAgent-role-greeting-human-deltav"));
    }
}


