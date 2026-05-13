using System.Linq;
using System.Numerics;
using Content.Client.Message;
using Content.Client.UserInterface.RichText; // DeltaV - Limit what tags can be used in custom objective summaries
using Content.Shared.GameTicking;
using Content.Shared.RoundEnd;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.RichText; // DeltaV - Limit what tags can be used in custom objective summaries
using Robust.Shared.Utility;
using Robust.Client.UserInterface;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.RoundEnd
{
    public sealed class RoundEndSummaryWindow : DefaultWindow
    {
        private readonly IEntityManager _entityManager;
        public int RoundId;
        private bool _hasCommended;

        public RoundEndSummaryWindow(string gm, string roundEnd, TimeSpan roundTimeSpan, int roundId,
            RoundEndMessageEvent.RoundEndPlayerInfo[] info, IEntityManager entityManager,
            string? localPlayerOOCName = null)
        {
            _entityManager = entityManager;

            MinSize = SetSize = new Vector2(520, 580);

            Title = Loc.GetString("round-end-summary-window-title");

            // The round end window is split into tabs: round summary, player manifest, and commendation.

            RoundId = roundId;
            var roundEndTabs = new TabContainer();
            roundEndTabs.AddChild(MakeRoundEndSummaryTab(gm, roundEnd, roundTimeSpan, roundId));
            roundEndTabs.AddChild(MakePlayerManifestTab(info));
            roundEndTabs.AddChild(MakeCommendationTab(info, localPlayerOOCName));

            ContentsContainer.AddChild(roundEndTabs);

            OpenCenteredRight();
            MoveToFront();
        }

        private BoxContainer MakeRoundEndSummaryTab(string gamemode, string roundEnd, TimeSpan roundDuration, int roundId)
        {
            var roundEndSummaryTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = Loc.GetString("round-end-summary-window-round-end-summary-tab-title")
            };

            var roundEndSummaryContainerScrollbox = new ScrollContainer
            {
                VerticalExpand = true,
                Margin = new Thickness(10)
            };
            var roundEndSummaryContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical
            };

            //Gamemode Name
            var gamemodeLabel = new RichTextLabel();
            var gamemodeMessage = new FormattedMessage();
            gamemodeMessage.AddMarkupOrThrow(Loc.GetString("round-end-summary-window-round-id-label", ("roundId", roundId)));
            gamemodeMessage.AddText(" ");
            gamemodeMessage.AddMarkupOrThrow(Loc.GetString("round-end-summary-window-gamemode-name-label", ("gamemode", gamemode)));
            gamemodeLabel.SetMessage(gamemodeMessage);
            roundEndSummaryContainer.AddChild(gamemodeLabel);

            //Duration
            var roundTimeLabel = new RichTextLabel();
            roundTimeLabel.SetMarkup(Loc.GetString("round-end-summary-window-duration-label",
                                                   ("hours", roundDuration.Hours),
                                                   ("minutes", roundDuration.Minutes),
                                                   ("seconds", roundDuration.Seconds)));
            roundEndSummaryContainer.AddChild(roundTimeLabel);

            //Round end text
            if (!string.IsNullOrEmpty(roundEnd))
            {
                var roundEndLabel = new RichTextLabel();
                // Begin DeltaV - Limit what tags can be used in custom objective summaries
                roundEndLabel.SetMessage(
                    FormattedMessage.FromMarkupPermissive(roundEnd),
                    [
                        typeof(BoldItalicTag),
                        typeof(BoldTag),
                        typeof(BulletTag),
                        typeof(ColorTag),
                        typeof(HeadingTag),
                        typeof(ItalicTag),
                        typeof(MonoTag)
                    ]
                );
                // End DeltaV - Limit what tags can be used in custom objective summaries
                roundEndSummaryContainer.AddChild(roundEndLabel);
            }

            roundEndSummaryContainerScrollbox.AddChild(roundEndSummaryContainer);
            roundEndSummaryTab.AddChild(roundEndSummaryContainerScrollbox);

            return roundEndSummaryTab;
        }

        private BoxContainer MakePlayerManifestTab(RoundEndMessageEvent.RoundEndPlayerInfo[] playersInfo)
        {
            var playerManifestTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = Loc.GetString("round-end-summary-window-player-manifest-tab-title")
            };

            var playerInfoContainerScrollbox = new ScrollContainer
            {
                VerticalExpand = true,
                Margin = new Thickness(10)
            };
            var playerInfoContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical
            };

            //Put observers at the bottom of the list. Put antags on top.
            var sortedPlayersInfo = playersInfo.OrderBy(p => p.Observer).ThenBy(p => !p.Antag);

            //Create labels for each player info.
            foreach (var playerInfo in sortedPlayersInfo)
            {
                var hBox = new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal,
                };

                var playerInfoText = new RichTextLabel
                {
                    VerticalAlignment = VAlignment.Center,
                    VerticalExpand = true,
                };

                if (playerInfo.PlayerNetEntity != null)
                {
                    hBox.AddChild(new SpriteView(playerInfo.PlayerNetEntity.Value, _entityManager)
                        {
                            OverrideDirection = Direction.South,
                            VerticalAlignment = VAlignment.Center,
                            SetSize = new Vector2(32, 32),
                            VerticalExpand = true,
                        });
                }

                if (playerInfo.PlayerICName != null)
                {
                    if (playerInfo.Observer)
                    {
                        playerInfoText.SetMarkup(
                            Loc.GetString("round-end-summary-window-player-info-if-observer-text",
                                          ("playerOOCName", playerInfo.PlayerOOCName),
                                          ("playerICName", playerInfo.PlayerICName)));
                    }
                    else
                    {
                        //TODO: On Hover display a popup detailing more play info.
                        //For example: their antag goals and if they completed them sucessfully.
                        var icNameColor = playerInfo.Antag ? "red" : "white";
                        playerInfoText.SetMarkup(
                            Loc.GetString("round-end-summary-window-player-info-if-not-observer-text",
                                ("playerOOCName", playerInfo.PlayerOOCName),
                                ("icNameColor", icNameColor),
                                ("playerICName", playerInfo.PlayerICName),
                                ("playerRole", Loc.GetString(playerInfo.Role))));
                    }
                }
                hBox.AddChild(playerInfoText);
                playerInfoContainer.AddChild(hBox);
            }

            playerInfoContainerScrollbox.AddChild(playerInfoContainer);
            playerManifestTab.AddChild(playerInfoContainerScrollbox);

            return playerManifestTab;
        }

        /// <summary>
        /// Creates the "Crew Commendation" tab where players can give a +1 RP like to one player.
        /// </summary>
        private BoxContainer MakeCommendationTab(
            RoundEndMessageEvent.RoundEndPlayerInfo[] playersInfo,
            string? localPlayerOOCName)
        {
            var commendTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = "Оцінка екіпажу" // Tab title
            };

            // Header
            var headerLabel = new RichTextLabel();
            headerLabel.SetMarkup("[bold]Оцінка екіпажу[/bold]\nОберіть гравця, який найкраще відіграв цей раунд.");
            commendTab.AddChild(headerLabel);

            commendTab.AddChild(new Control { MinSize = new Vector2(0, 8) }); // spacer

            // Search / filter
            var searchBox = new LineEdit
            {
                PlaceHolder = "Пошук за ім'ям...",
                HorizontalExpand = true,
            };
            commendTab.AddChild(searchBox);

            commendTab.AddChild(new Control { MinSize = new Vector2(0, 4) }); // spacer

            // Player list (scrollable)
            var scrollContainer = new ScrollContainer
            {
                VerticalExpand = true,
                Margin = new Thickness(4),
            };

            var playerListContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
            };

            // Status label (shows result after voting)
            var statusLabel = new RichTextLabel
            {
                HorizontalExpand = true,
            };
            statusLabel.SetMarkup("");

            // Filter out observers; only show players who actually played
            var eligiblePlayers = playersInfo
                .Where(p => !p.Observer && p.PlayerICName != null)
                .OrderBy(p => p.PlayerICName)
                .ToArray();

            // Build player buttons
            var playerButtons = new List<(BoxContainer Container, string ICName, string OOCName)>();

            foreach (var playerInfo in eligiblePlayers)
            {
                // Skip self
                if (localPlayerOOCName != null &&
                    string.Equals(playerInfo.PlayerOOCName, localPlayerOOCName, StringComparison.OrdinalIgnoreCase))
                    continue;

                var row = new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal,
                    HorizontalExpand = true,
                    Margin = new Thickness(2),
                };

                // Player sprite
                if (playerInfo.PlayerNetEntity != null)
                {
                    row.AddChild(new SpriteView(playerInfo.PlayerNetEntity.Value, _entityManager)
                    {
                        OverrideDirection = Direction.South,
                        VerticalAlignment = VAlignment.Center,
                        SetSize = new Vector2(32, 32),
                    });
                }

                // Player name label
                var nameLabel = new RichTextLabel
                {
                    VerticalAlignment = VAlignment.Center,
                    HorizontalExpand = true,
                };
                var roleText = Loc.GetString(playerInfo.Role);
                var commendationsText = $" [color=gold]\\[+{playerInfo.TotalCommendations} ({playerInfo.RoundCommendations} за раунд)\\][/color]";
                nameLabel.SetMarkup($"[bold]{playerInfo.PlayerICName}[/bold] ({playerInfo.PlayerOOCName}) — {roleText}{commendationsText}");
                row.AddChild(nameLabel);

                // Commend button
                var oocName = playerInfo.PlayerOOCName;
                var commendButton = new Button
                {
                    Text = "👍",
                    ToolTip = $"Похвалити {playerInfo.PlayerICName}",
                    MinSize = new Vector2(40, 30),
                    VerticalAlignment = VAlignment.Center,
                    Visible = oocName != localPlayerOOCName // Cannot commend yourself
                };

                commendButton.OnPressed += _ =>
                {
                    if (_hasCommended)
                    {
                        statusLabel.SetMarkup("[color=yellow]Ви вже проголосували цього раунду![/color]");
                        return;
                    }

                    _hasCommended = true;

                    // Disable all buttons
                    foreach (var (_, _, _) in playerButtons)
                    {
                        // We'll disable via a flag check in the handler
                    }

                    // Send network message
                    _entityManager.EntityNetManager?.SendSystemNetworkMessage(new CommendPlayerMessage(oocName));
                    statusLabel.SetMarkup($"[color=green]Ви похвалили цього гравця! Дякуємо за голос![/color]");
                };

                row.AddChild(commendButton);
                playerListContainer.AddChild(row);
                playerButtons.Add((row, playerInfo.PlayerICName ?? "", playerInfo.PlayerOOCName));
            }

            // Search filter logic
            searchBox.OnTextChanged += args =>
            {
                var filter = args.Text.Trim().ToLowerInvariant();
                foreach (var (container, icName, oocName) in playerButtons)
                {
                    container.Visible = string.IsNullOrEmpty(filter) ||
                                        icName.ToLowerInvariant().Contains(filter) ||
                                        oocName.ToLowerInvariant().Contains(filter);
                }
            };

            scrollContainer.AddChild(playerListContainer);
            commendTab.AddChild(scrollContainer);
            commendTab.AddChild(new Control { MinSize = new Vector2(0, 4) }); // spacer
            commendTab.AddChild(statusLabel);

            return commendTab;
        }
    }

}
