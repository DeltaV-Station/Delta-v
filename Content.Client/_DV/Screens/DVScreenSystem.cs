using Content.Shared._DV.Screens;

namespace Content.Client._DV.Screens;

public sealed class DVScreenSystem : DVSharedScreenSystem
{
    [Dependency] private readonly DVTextVisualsSystem _textVisuals = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DVScreenComponent, AfterAutoHandleStateEvent>(OnScreenState);
    }

    private void OnScreenState(Entity<DVScreenComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateVisuals(ent);

        switch (ent.Comp.Content)
        {
            case DVScreenContent.Text:
                Text(ent);
                break;
            case DVScreenContent.CurrentTime:
                CurrentTime(ent);
                break;
            case DVScreenContent.EstimatedTimeOfArrival:
                EstimatedTimeOfArrival(ent);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Text(Entity<DVScreenComponent> ent)
    {
        _textVisuals.SetText(ent.Owner, ent.Comp.Line1, ent.Comp.Line2);
    }

    private void CurrentTime(Entity<DVScreenComponent> ent)
    {
        _textVisuals.SetText(ent.Owner, "TIME", "69:69");
    }

    private void EstimatedTimeOfArrival(Entity<DVScreenComponent> ent)
    {
        _textVisuals.SetText(ent.Owner, "-ETA-", "69:69");
    }
}
