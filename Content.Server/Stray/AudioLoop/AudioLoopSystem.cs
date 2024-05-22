//AudioParams auSt = new AudioParams();
//auSt.Loop = true;
//AudioComponent audi = _audio.SetupAudio(uid, audiCom.fileName, auSt);
//audi.StartPlaying();
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Content.Shared.Interaction;
using Robust.Shared.Audio.Effects;
using Content.Shared.Stray.AudioLoop;
using JetBrains.Annotations;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;



namespace Content.Server.Stray.AudioLoop;

[UsedImplicitly]
public sealed class DiceSystem : SharedAudioLoopSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;


    public override void ActToggle(EntityUid uid, AudioLoopComponent audiCom, UseInHandEvent args){
        audiCom.act = !audiCom.act;

        if(audiCom.act==false){
            _audio.Stop(audiCom.ent, audiCom.auC);
        }else{
            (EntityUid Entity, AudioComponent Component)? res = _audio.PlayPvs(audiCom.sound, uid, new AudioParams(0, 1, SharedAudioSystem.DefaultSoundRange, 3, 1, true, 0f));
            if(res!=null){
                audiCom.ent = res.Value.Entity;
                audiCom.auC = res.Value.Component;
            }
        }
    }

    //public override void OnSpawn(EntityUid uid, AudioLoopComponent audiCom, MapInitEvent args){
    //    //AudioParams auSt = new(0, 1, SharedAudioSystem.DefaultSoundRange, 1, 1, true, 0f);//new AudioParams();
    //    //auSt.Loop = true;
    //    //auSt.RolloffFactor = 1;
    //    ////source.RolloffFactor = audioParams.RolloffFactor;
    //    //auSt.MaxDistance = 15;
    //    //auSt.ReferenceDistance = 1;
    //    //AudioComponent audi = _audio.SetupAudio(uid, audiCom.fileName, auSt);
    //    //audi.StartPlaying();
    //    (EntityUid Entity, AudioComponent Component)? res = _audio.PlayPvs(audiCom.sound, uid,  new AudioParams(0, 1, SharedAudioSystem.DefaultSoundRange, 1, 1, true, 0f));
    //    if(res!=null){
    //        audiCom.ent = res.Value.Entity;
    //        audiCom.auC = res.Value.Component;
    //    }
    //}

}
