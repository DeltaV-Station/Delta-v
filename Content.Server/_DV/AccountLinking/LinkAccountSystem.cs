using Content.Server.GameTicking;

namespace Content.Server._DV.AccountLinking;

public sealed class LinkAccountSystem : EntitySystem
{

    [Dependency] private readonly LinkAccountManager _linkAccount = default!;


    public override void Initialize()
    {
        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnGameRunLevelChanged);

        ReloadPatrons();
    }

    private void OnGameRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        switch (ev.New)
        {
            case GameRunLevel.InRound:
                break;
            case GameRunLevel.PreRoundLobby:
            case GameRunLevel.PostRound:
                ReloadPatrons();
                break;
        }
    }

    private async void ReloadPatrons()
    {
        try
        {
            await _linkAccount.RefreshAllPatrons();
            _linkAccount.SendPatronsToAll();
        }
        catch (Exception e)
        {
            Log.Error($"Error reloading Patrons list:\n{e}");
        }
    }
}
