using Content.Server.Mind;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared._DV.Mail;

namespace Content.Server._DV.Objectives.Systems;

public sealed class MailFraudObjectiveSystem : EntitySystem
{
    [Dependency] private readonly CounterConditionSystem _counterCondition = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MailComponent, MailOpenedEvent>(OnMailOpened);
    }

    private void OnMailOpened(Entity<MailComponent> ent, ref MailOpenedEvent args)
    {
        if (!args.Fraudulent)
            return;

        if (args.Opener is not { } opener
            || !_mind.TryGetMind(opener, out _, out var mind))
            return;

        foreach (var obj in mind.Objectives)
        {
            if (HasComp<MailFraudConditionComponent>(obj))
            {
                _counterCondition.IncreaseCount(obj);
            }
        }
    }
}
