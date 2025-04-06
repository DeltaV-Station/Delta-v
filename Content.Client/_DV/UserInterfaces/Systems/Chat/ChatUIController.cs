using System.Linq;
using System.Text.RegularExpressions;
using Content.Client.CharacterInfo;
using Content.Shared._DV.CCVars;
using Content.Shared.Dataset;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using static Content.Client.CharacterInfo.CharacterInfoSystem;

namespace Content.Client.UserInterface.Systems.Chat;

public sealed partial class ChatUIController : IOnSystemChanged<CharacterInfoSystem>
{
    /// <summary>
    ///     Gets Invoked whenever the autofilled highlights have changed.
    ///     Used to populate the preview in the channel selector window.
    /// </summary>
    public event Action<string>? OnAutoHighlightsUpdated;

    [UISystemDependency] private readonly CharacterInfoSystem _characterInfo = default!;

    /// <summary>
    ///     A list of words to be highlighted in the chatbox.
    ///     User-specified.
    /// </summary>
    private readonly List<string> _highlights = [];

    /// <summary>
    ///     A list of words to be highlighted in the chatbox.
    ///     Auto-generated from users's character information.
    /// </summary>
    private readonly List<string> _autoHighlights = [];

    /// <summary>
    ///     The color (hex) in witch the words will be highlighted as.
    /// </summary>
    private string? _highlightsColor;

    private bool _autoFillHighlightsEnabled;

    private void InitializeChatHighlights()
    {

        _player.LocalPlayerAttached += _ => _characterInfo.RequestCharacterInfo();
        _player.LocalPlayerDetached += _ => _characterInfo.RequestCharacterInfo();

        _config.OnValueChanged(DCCVars.ChatAutoFillHighlights, value => { _autoFillHighlightsEnabled = value; UpdateHighlights(); });
        _autoFillHighlightsEnabled = _config.GetCVar(DCCVars.ChatAutoFillHighlights);

        _config.OnValueChanged(DCCVars.ChatHighlightsColor, value => _highlightsColor = value);
        _highlightsColor = _config.GetCVar(DCCVars.ChatHighlightsColor);

        _config.OnValueChanged(DCCVars.ChatHighlights, UpdateHighlights);
        UpdateHighlights(_config.GetCVar(DCCVars.ChatHighlights));
    }


    public void OnSystemLoaded(CharacterInfoSystem system)
    {
        system.OnCharacterUpdate += UpdateAutoHighlights;
    }

    public void OnSystemUnloaded(CharacterInfoSystem system)
    {
        system.OnCharacterUpdate -= UpdateAutoHighlights;
    }

    private void UpdateAutoHighlights(CharacterData data)
    {
        var (_, job, _, _, entityName) = data;

        _autoHighlights.Clear();

        // If the character has a normal name (eg. "Name Surname" and not "Name Initial Surname" or a particular species name)
        // subdivide it so that the name and surname individually get highlighted.
        if (entityName.Count(c => c == ' ') == 1)
            _autoHighlights.AddRange(entityName.Split(' '));
        _autoHighlights.Add(entityName);

        var jobKey = "ChatHighlight" + job.Replace(" ", "");
        if (_prototypeManager.TryIndex<LocalizedDatasetPrototype>(jobKey, out var jobMatches))
            _autoHighlights.AddRange(jobMatches.Values.Select(Loc.GetString));
        else
            _sawmill.Debug("Missing LocalizedDataset for Job: " + jobKey);
        UpdateHighlights();
    }

    public void UpdateHighlights(string? newHighlights = null)
    {
        var configuredHighlights = _config.GetCVar(DCCVars.ChatHighlights);
        var highlights = newHighlights ?? configuredHighlights;
        // Save the newly provided list of highlights if different.
        if (newHighlights is not null && !string.Equals(configuredHighlights, highlights, StringComparison.CurrentCultureIgnoreCase))
        {
            _config.SetCVar(DCCVars.ChatHighlights, highlights);
            _config.SaveToFile();
        }

        var effectiveAutoHighlights = _autoFillHighlightsEnabled
            ? string.Join("\n", _autoHighlights)
            : string.Empty;
        OnAutoHighlightsUpdated?.Invoke(effectiveAutoHighlights);

        // If `highlights` is an empty string, this gives a single empty string, which breaks stuff, so check for that separately when adding it to `_highlights`
        var allHighlights = _autoFillHighlightsEnabled
            ? highlights.Split("\n").Concat(_autoHighlights)
            : highlights.Split("\n");

        _highlights.Clear();

        void AddHighlights(IEnumerable<string> highlights)
        {
            foreach (var highlight in highlights)
            {
                if (string.IsNullOrWhiteSpace(highlight))
                    continue;
                // Use `"` as layman symbol for Regex `\b`, ignore all other special sequences
                // (Without that escape, a name like `Robert'); DROP TABLE users; --` breaks all messsages)
                // Turn `\` into `\\` or else it'll escape the tags inside the actual chat message for reasons I can barely intuit but not explain.
                _highlights.Add(Regex.Escape(highlight.Replace(@"\", @"\\")).Replace("\"", "\\b"));
            }
        }

        AddHighlights(highlights.Split("\n"));
        if (_autoFillHighlightsEnabled)
            AddHighlights(_autoHighlights);

        // Arrange the list in descending order so that when highlighting,
        // the full word (eg. "Security") appears before the abbreviation (eg. "Sec").
        _highlights.Sort((x, y) => y.Length.CompareTo(x.Length));
    }
}
