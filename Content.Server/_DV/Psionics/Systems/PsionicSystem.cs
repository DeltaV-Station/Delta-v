using Content.Server._DV.Psionics.UI;
using Content.Server.EUI;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Systems;
using Content.Shared.EntityTable;
using Content.Shared.GameTicking;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._DV.Psionics.Systems;

public sealed partial class PsionicSystem : SharedPsionicSystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PotentialPsionicComponent, PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);

        InitializeItems();
    }

    private void OnPlayerSpawnComplete(Entity<PotentialPsionicComponent> potPsionic, ref PlayerSpawnCompleteEvent args)
    {
        if (RollChance(potPsionic))
            _euiManager.OpenEui(new AcceptPsionicsEui(potPsionic, this), args.Player);
    }
}
