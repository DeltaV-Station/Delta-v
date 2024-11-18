using Robust.Shared.Audio.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Content.Shared.Interaction;
using Robust.Shared.Audio.Effects;
using Robust.Shared.GameObjects;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;

namespace Content.Shared.Stray.AudioLoop;

public abstract partial class SharedAudioLoopSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();
        //_audio.
        //InitializeEffect();
        //ZOffset = CfgManager.GetCVar(CVars.AudioZOffset);
        //Subs.CVar(CfgManager, CVars.AudioZOffset, SetZOffset);
        //SubscribeLocalEvent<AudioComponent, ComponentGetStateAttemptEvent>(OnAudioGetStateAttempt);
        //SubscribeLocalEvent<AudioLoopComponent, MapInitEvent>(OnSpawn);
        SubscribeLocalEvent<AudioLoopComponent, UseInHandEvent>(ActToggle);
    }
    //(EntityUid uid, string? fileName, AudioParams? audioParams, TimeSpan? length = null)
    //public virtual void OnSpawn(EntityUid uid, AudioLoopComponent audiCom, MapInitEvent args){
//
    //}

    public virtual void ActToggle(EntityUid uid, AudioLoopComponent audiCom, UseInHandEvent args){

    }
}
