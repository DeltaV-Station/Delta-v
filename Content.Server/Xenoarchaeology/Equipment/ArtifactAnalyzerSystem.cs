using System;
using Content.Server.Research.Systems;
using Content.Server.Xenoarchaeology.Artifact;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Xenoarchaeology.BUI;
using Content.Shared.Xenoarchaeology.Equipment;
using Content.Shared.Xenoarchaeology.Equipment.Components;

using Robust.Server.GameObjects;
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

        var sumResearch = 0;
        var sumGlimmer = 0;
        ArtifactAnalyzerComponent? analyzer = null;
        if (ent.Comp.AnalyzerEntity is { } analyzerNetEntity)
        {
            var analyzerEntityUid = GetEntity(analyzerNetEntity); // Convert NetEntity to EntityUid
            TryComp<ArtifactAnalyzerComponent>(analyzerEntityUid, out analyzer);
        }

        foreach (var node in _xenoArtifact.GetAllNodes(artifact.Value))
        {
            var research = _xenoArtifact.GetResearchValue(node);
            _xenoArtifact.SetConsumedResearchValue(node, node.Comp.ConsumedResearchValue + research);

            if (analyzer != null)
            {
                sumGlimmer += (int)(research / (float)analyzer.ExtractRatio);
                research = (int)(research * GetGlimmerMultiplier(analyzer));
            }

            sumResearch += research;
        }
        if (analyzer != null)
        {
            UpdateClientUI(ent, analyzer);
        }


        if (sumResearch <= 0)
            return;

        _glimmerSystem.Glimmer += sumGlimmer; // DeltaV - Add glimmer based on extracted points.    
        _research.ModifyServerPoints(server.Value, sumResearch, serverComponent);
        _audio.PlayPvs(ent.Comp.ExtractSound, artifact.Value);
        _popup.PopupEntity(Loc.GetString("analyzer-artifact-extract-popup"), artifact.Value, PopupType.Large);
    }

    private void UpdateClientUI(EntityUid console, ArtifactAnalyzerComponent analyzer)
    {
        var glimmer = _glimmerSystem.Glimmer;
        var uiSystem = EntityManager.System<UserInterfaceSystem>();
        uiSystem.SetUiState(console, ArtifactAnalyzerUiKey.Key,
            new AnalysisConsoleBoundUserInterfaceState(GetGlimmerMultiplier(analyzer), (float)analyzer.ExtractRatio));
    }


    private float GetGlimmerMultiplier(ArtifactAnalyzerComponent comp)
    {
        return 1 + (MathF.Pow(_glimmerSystem.Glimmer / 1000f, 2f) * comp.PointGlimmerMultiplier);
    }
}

