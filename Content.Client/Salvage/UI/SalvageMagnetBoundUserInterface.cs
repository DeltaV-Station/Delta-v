using System.Linq;
using Content.Client.Message;
using Content.Shared._DV.Salvage.Systems; // DeltaV
using Content.Shared.Salvage;
using Content.Shared.Salvage.Magnet;
using Robust.Client.Player; // DeltaV
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Content.Shared._DV.Salvage.Magnet; //DeltaV
using Content.Client._DV.Salvage.UI; //DeltaV

namespace Content.Client.Salvage.UI;

public sealed class SalvageMagnetBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!; // DeltaV

    private readonly MiningPointsSystem _points; // DeltaV

    private MagnetOfferingWindow? _window; //DeltaV - use MagnetOfferingWindow rather than base OfferingWindow

    public SalvageMagnetBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
        _points = _entManager.System<MiningPointsSystem>(); // DeltaV
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindowCenteredLeft<MagnetOfferingWindow>(); //DeltaV - use MagnetOfferingWindow rather than OfferingWindow
        _window.Title = Loc.GetString("salvage-magnet-window-title");
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not SalvageMagnetBoundUserInterfaceState current || _window == null)
            return;

        _window.ClearOptions();

        var salvageSystem = _entManager.System<SharedSalvageSystem>();
        _window.NextOffer = current.NextOffer;
        _window.Progression = current.EndTime ?? TimeSpan.Zero;
        _window.Claimed = current.EndTime != null;
        _window.Cooldown = current.Cooldown;
        _window.ProgressionCooldown = current.Duration;

        // BEGIN DeltaV - start of magnet release additions
        _window.ReleaseTime = current.ReleaseTime;
        _window.Duration = current.Duration;
        _window.SetButtonPressed(_ =>
        {
            SendMessage(new MagnetReleaseEvent());
        });
        // END DeltaV

        for (var i = 0; i < current.Offers.Count; i++)
        {
            var seed = current.Offers[i];
            var offer = salvageSystem.GetSalvageOffering(seed);
            var option = new OfferingWindowOption();
            option.MinWidth = 210f;
            option.Disabled = current.EndTime != null;
            option.Claimed = current.ActiveSeed == seed;
            var claimIndex = i;

            option.ClaimPressed += _ =>
            {
                SendMessage(new MagnetClaimOfferEvent
                {
                    Index = claimIndex
                });
            };

            switch (offer)
            {
                case AsteroidOffering asteroid:
                    option.Title = Loc.GetString($"dungeon-config-proto-{asteroid.Id}");
                    var layerKeys = asteroid.MarkerLayers.Keys.ToList();
                    layerKeys.Sort();

                    foreach (var resource in layerKeys)
                    {
                        var count = asteroid.MarkerLayers[resource] * 2; //DeltaV - Double count to reflect higher concentration of ore in generator

                        var container = new BoxContainer
                        {
                            Orientation = BoxContainer.LayoutOrientation.Horizontal,
                            HorizontalExpand = true,
                        };

                        var resourceLabel = new Label
                        {
                            Text = Loc.GetString("salvage-magnet-resources",
                                ("resource", resource)),
                            HorizontalAlignment = Control.HAlignment.Left,
                        };

                        var countLabel = new Label
                        {
                            Text = Loc.GetString("salvage-magnet-resources-count", ("count", count)),
                            HorizontalAlignment = Control.HAlignment.Right,
                            HorizontalExpand = true,
                        };

                        container.AddChild(resourceLabel);
                        container.AddChild(countLabel);

                        option.AddContent(container);
                    }

                    break;
                case DebrisOffering debris:
                    option.Title = Loc.GetString($"salvage-magnet-debris-{debris.Id}");
                    //START DeltaV - Add dynamic debris loot
                    int scrapCount = 0;
                    int valuablesCount = 0;
                    int arcanaCount = 0;

                    switch (debris.LootId)
                    {
                        case "SpaceDebrisLootRegular":
                            scrapCount = 2;
                            valuablesCount = 2;
                            arcanaCount = 2;
                            break;
                        case "SpaceDebrisLootScrap":
                            scrapCount = 4;
                            valuablesCount = 1;
                            arcanaCount = 1;
                            break;
                        case "SpaceDebrisLootValuables":
                            scrapCount = 1;
                            valuablesCount = 4;
                            arcanaCount = 1;
                            break;
                        case "SpaceDebrisLootArcana":
                            scrapCount = 1;
                            valuablesCount = 1;
                            arcanaCount = 4;
                            break;
                    }

                    AddDebrisLootContent("LootScrap", scrapCount);
                    AddDebrisLootContent("LootValuables", valuablesCount);
                    AddDebrisLootContent("LootArcana", arcanaCount);

                    void AddDebrisLootContent(string debrisLoot, int count)
                    {
                        var debrisContainer = new BoxContainer
                        {
                            Orientation = BoxContainer.LayoutOrientation.Horizontal,
                            HorizontalExpand = true,
                        };

                        var debrisResourceLabel = new Label
                        {
                            Text = Loc.GetString("salvage-magnet-debris-loot",
                                ("loot", debrisLoot)),
                            HorizontalAlignment = Control.HAlignment.Left,
                        };

                        var debrisCountLabel = new Label
                        {
                            Text = Loc.GetString("salvage-magnet-resources-count", ("count", count)),
                            HorizontalAlignment = Control.HAlignment.Right,
                            HorizontalExpand = true,
                        };

                        debrisContainer.AddChild(debrisResourceLabel);
                        debrisContainer.AddChild(debrisCountLabel);

                        option.AddContent(debrisContainer);
                    }
                    //END DeltaV
                    break;

                /* case SalvageOffering salvage: // BEGIN DeltaV - we no longer use Salvage wrecks, instead incorporating them into magnet pulls
                    option.Title = Loc.GetString($"salvage-map-wreck");

                    var salvContainer = new BoxContainer
                    {
                        Orientation = BoxContainer.LayoutOrientation.Horizontal,
                        HorizontalExpand = true,
                    };

                    var sizeLabel = new Label
                    {
                        Text = Loc.GetString("salvage-map-wreck-desc-size"),
                        HorizontalAlignment = Control.HAlignment.Left,
                    };

                    var sizeValueLabel = new RichTextLabel
                    {
                        HorizontalAlignment = Control.HAlignment.Right,
                        HorizontalExpand = true,
                    };
                    sizeValueLabel.SetMarkup(Loc.GetString(salvage.SalvageMap.SizeString));

                    salvContainer.AddChild(sizeLabel);
                    salvContainer.AddChild(sizeValueLabel);

                    option.AddContent(salvContainer);
                    break;
                END DeltaV  */
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _window.AddOption(option);
        }
    }
}
