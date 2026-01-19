using System.Linq;
using Content.Client._DV.UserInterfaces.Systems.SignLanguage;
using Content.Client.Chat.TypingIndicator;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Shared._DV.SignLanguage;
using Content.Shared._DV.SignLanguage.Prototypes;
using Content.Shared.Chat.TypingIndicator;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Input;
using Content.Shared.Popups;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

// ReSharper disable once CheckNamespace
namespace Content.Client.UserInterface.Systems.SignLanguage;

[UsedImplicitly]
public sealed class SignLanguageUIController : UIController, IOnStateChanged<GameplayState>
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private SimpleRadialMenu? _currentMenu;
    private SignLanguagePreviewOverlay? _previewOverlay;
    private TypingIndicatorSystem? _typing;
    private SharedHandsSystem? _hands;
    private SharedPopupSystem? _popup;

    private static readonly ProtoId<TypingIndicatorPrototype> SignTypingIndicator = "aac";

    // Track the current selection state
    private SignTopicPrototype? _selectedTopic;
    private SignEventPrototype? _selectedEvent;
    private SignIntentPrototype? _selectedIntent;
    private SignIntensityPrototype? _selectedIntensity;

    // Menu stage tracking
    private enum MenuStage : byte
    {
        Topic,
        Event,
        Intent,
        Intensity
    }

    // This isn't used right now, might be later
#pragma warning disable CS0414 // Field is assigned but its value is never used
    private MenuStage _currentStage = MenuStage.Topic;
#pragma warning restore CS0414 // Field is assigned but its value is never used
    private bool _isActive;

    private static readonly Dictionary<SignTopicCategory, (string Tooltip, SpriteSpecifier Sprite)> TopicCategoryInfo =
        new()
        {
            [SignTopicCategory.People] = ("sign-menu-category-people",
                new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/SignLanguage/people.png"))),
            [SignTopicCategory.Locations] = ("sign-menu-category-locations",
                new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/SignLanguage/location.png"))),
            [SignTopicCategory.Objects] = ("sign-menu-category-objects",
                new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/SignLanguage/object.png"))),
            [SignTopicCategory.General] = ("sign-menu-category-general",
                new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/SignLanguage/general.png"))),
        };

    public void OnStateEntered(GameplayState state)
    {
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OpenSignLanguageMenu,
                InputCmdHandler.FromDelegate(_ => ToggleSignLanguageMenu(false)))
            .Bind(ContentKeyFunctions.SignLanguageSubmit,
                new PointerInputCmdHandler(HandleSendSign))
            .Bind(ContentKeyFunctions.SignLanguageCancel,
                new PointerInputCmdHandler(HandleCancelSign))
            .Register<SignLanguageUIController>();

        _typing = EntityManager.System<TypingIndicatorSystem>();
        _hands = EntityManager.System<SharedHandsSystem>();
        _popup = EntityManager.System<SharedPopupSystem>();
        _previewOverlay = new SignLanguagePreviewOverlay();
    }

    public void OnStateExited(GameplayState state)
    {
        CommandBinds.Unregister<SignLanguageUIController>();

        // Clean up overlay
        if (_previewOverlay != null && _overlay.HasOverlay<SignLanguagePreviewOverlay>())
        {
            _overlay.RemoveOverlay(_previewOverlay);
        }

        _typing = null;
        _hands = null;
        _popup = null;
    }

    /// <summary>
    /// Checks if the local player has at least one free hand to perform sign language.
    /// </summary>
    private bool HasFreeHand()
    {
        var player = _player.LocalEntity;
        if (player == null || _hands == null)
            return false;

        return _hands.CountFreeHands(player.Value) >= 1;
    }

    private bool HandleSendSign(in PointerInputCmdHandler.PointerInputCmdArgs args)
    {
        // Only handle if we're in sign language mode and have at least topic + event + intent
        if (!_isActive || _selectedTopic == null || _selectedEvent == null || _selectedIntent == null)
            return false;

        if (args.State != BoundKeyState.Down)
            return false;

        SendSign();
        return true;
    }

    private bool HandleCancelSign(in PointerInputCmdHandler.PointerInputCmdArgs args)
    {
        // Only handle if we're in sign language mode
        if (!_isActive)
            return false;

        if (args.State != BoundKeyState.Down)
            return false;

        CloseAllMenus();
        return true;
    }

    private void ToggleSignLanguageMenu(bool centered)
    {
        if (_currentMenu == null)
        {
            // Check if the player has at least one free hand
            var player = _player.LocalEntity;

            if (player == null)
                return;

            // Needs to known sign language to open the menu
            if (!EntityManager.HasComponent<KnowsSignLanguageComponent>(player))
                return;

            if (!HasFreeHand())
            {
                _popup?.PopupClient(Loc.GetString("sign-language-need-free-hand"), player.Value);
                return;
            }

            ResetSelections();
            _isActive = true;

            // Show preview overlay
            if (_previewOverlay != null && !_overlay.HasOverlay<SignLanguagePreviewOverlay>())
            {
                _overlay.AddOverlay(_previewOverlay);
            }

            // Start typing indicator when menu opens
            UpdateTypingIndicator();

            ShowTopicMenu(centered);
        }
        else
        {
            CloseAllMenus();
        }
    }

    private void ResetSelections()
    {
        _selectedTopic = null;
        _selectedEvent = null;
        _selectedIntent = null;
        _selectedIntensity = null;
        _currentStage = MenuStage.Topic;

        // Update preview overlay
        _previewOverlay?.ClearPreview();
    }

    private void CloseAllMenus()
    {
        // Hide preview overlay
        if (_previewOverlay != null && _overlay.HasOverlay<SignLanguagePreviewOverlay>())
        {
            _overlay.RemoveOverlay(_previewOverlay);
        }

        if (_currentMenu == null)
        {
            _isActive = false;
            ResetSelections();
            ClearTypingIndicator();
            return;
        }

        _currentMenu.OnClose -= OnMenuClosed;
        _currentMenu.Close();
        _currentMenu = null;

        _isActive = false;
        ResetSelections();
        ClearTypingIndicator();
    }

    private void OnMenuClosed()
    {
        CloseAllMenus();
    }

    private void UpdateTypingIndicator()
    {
        _typing?.ClientAlternateTyping(SignTypingIndicator);
    }

    private void ClearTypingIndicator()
    {
        _typing?.ClientSubmittedChatText();
    }

    #region Menu 1 - Topic Selection

    private void ShowTopicMenu(bool centered)
    {
        _currentStage = MenuStage.Topic;

        var topics = _prototype.EnumeratePrototypes<SignTopicPrototype>()
            .OrderBy(t => t.Priority)
            .ToList();

        var models = ConvertTopicsToButtons(topics);

        _currentMenu = new SimpleRadialMenu();
        _currentMenu.SetButtons(models);
        _currentMenu.OnClose += OnMenuClosed;

        if (centered)
            _currentMenu.OpenCentered();
        else
            _currentMenu.OpenOverMouseScreenPosition();
    }

    private IEnumerable<RadialMenuOptionBase> ConvertTopicsToButtons(IEnumerable<SignTopicPrototype> topics)
    {
        // Group topics by category
        Dictionary<SignTopicCategory, List<RadialMenuOptionBase>> topicsByCategory = new();

        foreach (var topic in topics)
        {
            if (topic.Category == SignTopicCategory.Invalid || topic.Category == SignTopicCategory.All)
                continue;

            if (!topicsByCategory.TryGetValue(topic.Category, out var list))
            {
                list = new List<RadialMenuOptionBase>();
                topicsByCategory.Add(topic.Category, list);
            }

            var topicOption = new RadialMenuActionOption<SignTopicPrototype>(HandleTopicSelection, topic)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(topic.Icon),
                ToolTip = Loc.GetString(topic.Name)
            };
            list.Add(topicOption);
        }

        // Create nested category buttons
        var models = new List<RadialMenuOptionBase>();

        foreach (var (category, list) in topicsByCategory.OrderBy(x => (int)x.Key))
        {
            if (!TopicCategoryInfo.TryGetValue(category, out var categoryInfo))
                continue;

            var categoryOption = new RadialMenuNestedLayerOption(list)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(categoryInfo.Sprite),
                ToolTip = Loc.GetString(categoryInfo.Tooltip)
            };
            models.Add(categoryOption);
        }

        return models;
    }

    private void HandleTopicSelection(SignTopicPrototype topic)
    {
        _selectedTopic = topic;
        UpdatePreviewOverlay();
        UpdateTypingIndicator();
        CloseCurrentMenu();
        ShowEventMenu();
    }

    #endregion

    #region Menu 2 - Event Selection

    private void ShowEventMenu()
    {
        if (_selectedTopic == null)
        {
            CloseAllMenus();
            return;
        }

        _currentStage = MenuStage.Event;

        var events = _prototype.EnumeratePrototypes<SignEventPrototype>()
            .Where(e => e.ApplicableCategories.Count == 0 || e.ApplicableCategories.Contains(_selectedTopic.Category))
            .OrderBy(e => e.Priority)
            .ToList();

        var models = ConvertEventsToButtons(events);

        _currentMenu = new SimpleRadialMenu();
        _currentMenu.SetButtons(models);
        _currentMenu.OnClose += OnMenuClosed;
        _currentMenu.OpenOverMouseScreenPosition();
    }

    private IEnumerable<RadialMenuOptionBase> ConvertEventsToButtons(IEnumerable<SignEventPrototype> events)
    {
        var models = new List<RadialMenuOptionBase>();

        foreach (var eventProto in events)
        {
            var eventOption = new RadialMenuActionOption<SignEventPrototype>(HandleEventSelection, eventProto)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(eventProto.Icon),
                ToolTip = Loc.GetString(eventProto.Name)
            };
            models.Add(eventOption);
        }

        return models;
    }

    private void HandleEventSelection(SignEventPrototype eventProto)
    {
        _selectedEvent = eventProto;
        UpdatePreviewOverlay();
        UpdateTypingIndicator();
        CloseCurrentMenu();
        ShowIntentMenu();
    }

    #endregion

    #region Menu 3 - Intent Selection

    private void ShowIntentMenu()
    {
        if (_selectedTopic == null || _selectedEvent == null)
        {
            CloseAllMenus();
            return;
        }

        _currentStage = MenuStage.Intent;

        var intents = _prototype.EnumeratePrototypes<SignIntentPrototype>()
            .OrderBy(i => i.Priority)
            .ToList();

        var models = ConvertIntentsToButtons(intents);

        _currentMenu = new SimpleRadialMenu();
        _currentMenu.SetButtons(models);
        _currentMenu.OnClose += OnMenuClosed;
        _currentMenu.OpenOverMouseScreenPosition();
    }

    private IEnumerable<RadialMenuOptionBase> ConvertIntentsToButtons(IEnumerable<SignIntentPrototype> intents)
    {
        var models = new List<RadialMenuOptionBase>();

        foreach (var intent in intents)
        {
            var intentOption = new RadialMenuActionOption<SignIntentPrototype>(HandleIntentSelection, intent)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(intent.Icon),
                ToolTip = Loc.GetString(intent.Name)
            };
            models.Add(intentOption);
        }

        return models;
    }

    private void HandleIntentSelection(SignIntentPrototype intent)
    {
        _selectedIntent = intent;
        UpdatePreviewOverlay();
        UpdateTypingIndicator();
        CloseCurrentMenu();

        // Check if we should show intensity menu
        var intensities = _prototype.EnumeratePrototypes<SignIntensityPrototype>().ToList();

        if (intensities.Count > 1) // Only show if there are actual choices
        {
            ShowIntensityMenu();
        }
        else
        {
            // Use default intensity and complete the sign (user can now press Enter to send)
            _selectedIntensity = intensities.FirstOrDefault(i => i.IsDefault);
            UpdatePreviewOverlay();
        }
    }

    #endregion

    #region Menu 4 - Intensity Selection (Optional)

    private void ShowIntensityMenu()
    {
        if (_selectedTopic == null || _selectedEvent == null || _selectedIntent == null)
        {
            CloseAllMenus();
            return;
        }

        _currentStage = MenuStage.Intensity;

        var intensities = _prototype.EnumeratePrototypes<SignIntensityPrototype>()
            .OrderBy(i => i.Priority)
            .ToList();

        var models = ConvertIntensitiesToButtons(intensities);

        _currentMenu = new SimpleRadialMenu();
        _currentMenu.SetButtons(models);
        _currentMenu.OnClose += OnMenuClosed;
        _currentMenu.OpenOverMouseScreenPosition();
    }

    private IEnumerable<RadialMenuOptionBase> ConvertIntensitiesToButtons(IEnumerable<SignIntensityPrototype> intensities)
    {
        var models = new List<RadialMenuOptionBase>();

        foreach (var intensity in intensities)
        {
            var intensityOption = new RadialMenuActionOption<SignIntensityPrototype>(HandleIntensitySelection, intensity)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(intensity.Icon),
                ToolTip = Loc.GetString(intensity.Name)
            };
            models.Add(intensityOption);
        }

        return models;
    }

    private void HandleIntensitySelection(SignIntensityPrototype intensity)
    {
        _selectedIntensity = intensity;
        UpdatePreviewOverlay();
        UpdateTypingIndicator();
        CloseCurrentMenu();
        // Sign is now complete and preview shows it - user presses Enter to send
    }

    #endregion

    #region Preview Overlay Management

    private void UpdatePreviewOverlay()
    {
        _previewOverlay?.UpdatePreview(_selectedTopic, _selectedEvent, _selectedIntent, _selectedIntensity);
    }

    #endregion

    #region Sign Execution

    private void SendSign()
    {
        if (_selectedTopic == null || _selectedEvent == null || _selectedIntent == null)
        {
            CloseAllMenus();
            return;
        }

        // Get the default intensity if none was selected
        _selectedIntensity ??= _prototype.EnumeratePrototypes<SignIntensityPrototype>()
            .FirstOrDefault(i => i.IsDefault);

        // Raise the sign language event with the selections
        EntityManager.RaisePredictiveEvent(new PerformSignLanguageMessage(
            _selectedTopic.ID,
            _selectedEvent.ID,
            _selectedIntent.ID,
            _selectedIntensity?.ID
        ));

        CloseAllMenus();
    }

    #endregion

    private void CloseCurrentMenu()
    {
        if (_currentMenu == null)
            return;

        _currentMenu.OnClose -= OnMenuClosed;
        _currentMenu.Close();
        _currentMenu = null;
    }
}
