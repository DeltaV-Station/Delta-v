using Content.Server._DV.Psionics.UI;
using Content.Server.EUI;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Systems;
using Content.Shared.GameTicking;

namespace Content.Server._DV.Psionics.Systems;

public sealed partial class PsionicSystem : SharedPsionicSystem
{
    [Dependency] private readonly EuiManager _euiManager = default!;

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
