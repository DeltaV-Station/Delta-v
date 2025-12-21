using System; // DeltaV
using Content.Server.Research.Systems;
using Content.Server.Xenoarchaeology.Artifact;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;// DeltaV
using Content.Shared._DV.Xenoarchaeology.BUI;// DeltaV
using Content.Shared.Xenoarchaeology.Equipment;
using Content.Shared.Xenoarchaeology.Equipment.Components;

using Robust.Server.GameObjects; // DeltaV
using Robust.Shared.Audio.Systems;


namespace Content.Server.Xenoarchaeology.Equipment;

/// <inheritdoc />
public sealed class ArtifactAnalyzerSystem : SharedArtifactAnalyzerSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ResearchSystem _research = default!;
    [Dependency] private readonly XenoArtifactSystem _xenoArtifact = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!; // DeltaV
    [Dependency] private readonly ArtifactAnalyzerSystem _analyzerSystem = default!; //DeltaV

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnalysisConsoleComponent, AnalysisConsoleExtractButtonPressedMessage>(OnExtractButtonPressed);
    }

    private void OnExtractButtonPressed(Entity<AnalysisConsoleComponent> ent, ref AnalysisConsoleExtractButtonPressedMessage args)
    {
        if (!TryGetArtifactFromConsole(ent, out var artifact))
            return;

        if (!_research.TryGetClientServer(ent, out var server, out var serverComponent))
            return;

        // Begin DeltaV - Variables for extraction totals
        var sumResearch = 0;
        var sumGlimmer = 0;
        if (!_analyzerSystem.TryGetAnalyzer(ent, out var analyzer))
            return;
        // End DeltaV

        foreach (var node in _xenoArtifact.GetAllNodes(artifact.Value))
        {
            var research = _xenoArtifact.GetResearchValue(node);
            _xenoArtifact.SetConsumedResearchValue(node, node.Comp.ConsumedResearchValue + research);

            // Begin DeltaV - Only run if we have an artifact ready for extraction
            if (analyzer != null)
            {
                sumGlimmer += (int)(research / (float)analyzer.Value.Comp.ExtractRatio);
                research = (int)(research * GetGlimmerMultiplier(analyzer.Value.Comp));
            }
            // End DeltaV
            sumResearch += research;
        }
        UpdateClientUI(ent, analyzer!.Value); // DeltaV

        // 4-16-25: It's a sad day when a scientist makes negative 5k research
        if (sumResearch <= 0)
            return;

        _glimmerSystem.Glimmer += sumGlimmer; // DeltaV - Add glimmer based on extracted points.    
        _research.ModifyServerPoints(server.Value, sumResearch, serverComponent);
        _audio.PlayPvs(ent.Comp.ExtractSound, artifact.Value);
        _popup.PopupEntity(Loc.GetString("analyzer-artifact-extract-popup"), artifact.Value, PopupType.Large);
    }

    // DeltaV
    private void UpdateClientUI(EntityUid console, ArtifactAnalyzerComponent analyzer)
    {

        var uiSystem = EntityManager.System<UserInterfaceSystem>();
        uiSystem.SetUiState(console, ArtifactAnalyzerUiKey.Key,
            new AnalysisConsoleBoundUserInterfaceState(GetGlimmerMultiplier(analyzer), (float)analyzer.ExtractRatio));
    }

    // DeltaV
    private float GetGlimmerMultiplier(ArtifactAnalyzerComponent comp)
    {
        float normalizedGlimmer = Math.Clamp(_glimmerSystem.Glimmer / 1000f, 0, 1);
        //DeltaV - Prevents extreme glimmer multipliers
        return (float)(.5f + Math.Clamp(Math.Pow(normalizedGlimmer, 0.5f) + 1.5f * Math.Pow(normalizedGlimmer, 10f), 0f, 2.5f));

    }
}

