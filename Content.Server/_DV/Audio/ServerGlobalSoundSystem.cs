using Content.Shared.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Map;

namespace Content.Server.Audio;

/// <summary>
/// Extends upstream's Content.Server/Audio/ServerGlobalSoundSystem.cs.
/// </summary>
public sealed partial class ServerGlobalSoundSystem
{
    /// <summary>
    /// DeltaV - Plays a sound globally for all players, no matter where they are.
    /// </summary>
    /// <param name="specifier"></param>
    /// <param name="audioParams"></param>
    public void PlayGlobal(ResolvedSoundSpecifier specifier, AudioParams? audioParams = null)
    {
        var msg = new GameGlobalSoundEvent(specifier, audioParams);
        RaiseNetworkEvent(msg);
    }

    public void StopGlobalEventMusic(StationEventMusicType type)
    {
        var msg = new StopStationEventMusic(type);
        RaiseNetworkEvent(msg);
    }

    public void DispatchGlobalEventMusic(SoundSpecifier sound, StationEventMusicType type)
    {
        DispatchGlobalEventMusic(_audio.ResolveSound(sound), type);
    }

    public void DispatchGlobalEventMusic(ResolvedSoundSpecifier specifier, StationEventMusicType type)
    {
        var audio = AudioParams.Default.WithVolume(-8);
        var msg = new StationEventMusicEvent(specifier, type, audio);
        RaiseNetworkEvent(msg);
    }

    /// <summary>
    /// DeltaV - Plays a sound globally for all players on a specified Map.
    /// </summary>
    /// <param name="specifier"></param>
    /// <param name="audioParams"></param>
    public void PlayGlobalOnMap(MapId map, ResolvedSoundSpecifier specifier, AudioParams? audioParams = null)
    {
        var msg = new GameGlobalSoundEvent(specifier, audioParams);
        var filter = Filter.Empty().AddInMap(map);
        RaiseNetworkEvent(msg, filter);
    }

    public void StopMapEventMusic(MapId map, StationEventMusicType type)
    {
        var msg = new StopStationEventMusic(type);
        var filter = Filter.Empty().AddInMap(map);
        RaiseNetworkEvent(msg, filter);
    }

    public void DispatchMapEventMusic(MapId map, SoundSpecifier sound, StationEventMusicType type)
    {
        DispatchMapEventMusic(map, _audio.ResolveSound(sound), type);
    }

    public void DispatchMapEventMusic(MapId map, ResolvedSoundSpecifier specifier, StationEventMusicType type)
    {
        var audio = AudioParams.Default.WithVolume(-8);
        var msg = new StationEventMusicEvent(specifier, type, audio);
        var filter = Filter.Empty().AddInMap(map);
        RaiseNetworkEvent(msg, filter);
    }
}
