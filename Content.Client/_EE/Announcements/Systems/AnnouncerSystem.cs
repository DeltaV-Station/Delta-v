using System.Linq;
using Content.Client.Audio;
using Content.Shared._EE.Announcements.Events;
using Content.Shared._EE.Announcements.Systems;
using Content.Shared.CCVar;
using Content.Shared._Impstation.CCVar;
using Robust.Client.Audio;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Audio.Sources;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;

namespace Content.Client._EE.Announcements.Systems;

public sealed class AnnouncerSystem : SharedAnnouncerSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly IAudioManager _audioManager = default!;

    public List<IAudioSource> AnnouncerSources { get; } = new();
    public float AnnouncerVolume { get; private set; }


    public override void Initialize()
    {
        base.Initialize();

        AnnouncerVolume = _config.GetCVar(ImpCCVars.AnnouncerVolume) * 100f / ContentAudioSystem.AnnouncerMultiplier;

        _config.OnValueChanged(ImpCCVars.AnnouncerVolume, OnAnnouncerVolumeChanged); // Impstation: Namespacing
        _config.OnValueChanged(ImpCCVars.AnnouncerDisableMultipleSounds, OnAnnouncerDisableMultipleSounds);

        SubscribeNetworkEvent<AnnouncementSendEvent>(OnAnnouncementReceived);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _config.UnsubValueChanged(ImpCCVars.AnnouncerVolume, OnAnnouncerVolumeChanged); // Impstation: Namespacing
        _config.UnsubValueChanged(ImpCCVars.AnnouncerDisableMultipleSounds, OnAnnouncerDisableMultipleSounds);
    }


    private void OnAnnouncerVolumeChanged(float value)
    {
        AnnouncerVolume = value;

        foreach (var source in AnnouncerSources)
            source.Gain = AnnouncerVolume;
    }

    private void OnAnnouncerDisableMultipleSounds(bool value)
    {
        if (!value)
            return;

        foreach (var audioSource in AnnouncerSources.ToList())
        {
            audioSource.Dispose();
            AnnouncerSources.Remove(audioSource);
        }
    }

    private void OnAnnouncementReceived(AnnouncementSendEvent ev)
    {
        if (!ev.Recipients.Contains(_player.LocalSession!.UserId)
            || !_cache.TryGetResource<AudioResource>(GetAnnouncementPath(ev.AnnouncementId, ev.AnnouncerId),
                out var resource))
            return;

        var source = _audioManager.CreateAudioSource(resource);
        if (source == null)
            return;

        source.Gain = AnnouncerVolume * SharedAudioSystem.VolumeToGain(ev.AudioParams.Volume);
        source.Global = true;

        if (_config.GetCVar(ImpCCVars.AnnouncerDisableMultipleSounds)) // Impstation: Namespacing
        {
            foreach (var audioSource in AnnouncerSources.ToList())
            {
                audioSource.Dispose();
                AnnouncerSources.Remove(audioSource);
            }
        }

        foreach (var audioSource in AnnouncerSources.ToList().Where(audioSource => !audioSource.Playing))
        {
            audioSource.Dispose();
            AnnouncerSources.Remove(audioSource);
        }

        AnnouncerSources.Add(source);
        source.StartPlaying();
    }
}
