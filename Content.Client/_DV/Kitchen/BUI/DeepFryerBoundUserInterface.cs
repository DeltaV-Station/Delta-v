using System.Linq;
using Content.Client._DV.Kitchen.UI;
using Content.Shared._DV.Kitchen.BUI;
using Content.Shared._DV.Kitchen.Systems;
using Robust.Client.Player;
using Robust.Client.UserInterface;
namespace Content.Client._DV.Kitchen.BUI;

public sealed class DeepFryerBoundUserInterface : BoundUserInterface
{
    private DeepFryerWindow? _window;
    
    [Dependency]
    private IPlayerManager _player = default!;

    public DeepFryerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<DeepFryerWindow>();

        _window.OnFoodItemPressed += (item) =>
        {
            SendPredictedMessage(new DeepFryerTryEjectItemMessage(EntMan.GetNetEntity(item), EntMan.GetNetEntity(_player.LocalEntity)));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        
        if (_window is null || state is not DeepFryerBoundUserInterfaceState deepFryerState)
            return;
        
        _window.OilQuality = deepFryerState.OilQuality;
        _window.SolutionColor = deepFryerState.SolutionColor;
        _window.MinimumVolume = deepFryerState.MinimumVolume;
        _window.SolutionVolume = deepFryerState.SolutionVolume;
        _window.SolutionMaxVolume = deepFryerState.SolutionMaxVolume;
        _window.CookingItems = [..deepFryerState.CookingItems.Select(EntMan.GetEntity)];
        _window.IsPowered = deepFryerState.IsPowered;
        _window.Capacity = deepFryerState.Capacity;
    }
}
