using Content.Shared.Examine;

namespace Content.Shared.DeltaV.NanoChat;

/// <summary>
///     Base system for NanoChat functionality shared between client and server.
/// </summary>
public abstract class SharedNanoChatSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NanoChatCardComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<NanoChatCardComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (ent.Comp.Number == null)
        {
            args.PushMarkup(Loc.GetString("nanochat-card-examine-no-number"));
            return;
        }

        args.PushMarkup(Loc.GetString("nanochat-card-examine-number", ("number", $"{ent.Comp.Number:D4}")));
    }
}
