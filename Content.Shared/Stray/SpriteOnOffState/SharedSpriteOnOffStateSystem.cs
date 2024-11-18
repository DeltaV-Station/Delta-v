using Content.Shared.Clothing;
using Content.Shared.Interaction;

namespace Content.Shared.Stray.SpriteOnOffState;

public abstract class SharedSpriteOnOffStateSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpriteOnOffStateComponent, ActivateInWorldEvent>(OnAct);
        SubscribeLocalEvent<SpriteOnOffStateComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
    }
    private void OnAct(EntityUid uid, SpriteOnOffStateComponent component, ActivateInWorldEvent args)
    {
        ShangeIsOn(uid, component);
    }
    private void OnAfterHandleState(EntityUid uid, SpriteOnOffStateComponent component, ref AfterAutoHandleStateEvent args)
    {
        UpdateVisuals(uid, component);
    }
    public void ChIsOn(EntityUid uid, bool chTo, SpriteOnOffStateComponent? component = null){
        if (!Resolve(uid, ref component))
            return;
        component.IsOn = chTo;
        Dirty(uid, component);
        UpdateVisuals(uid, component);
    }
    protected virtual void UpdateVisuals(EntityUid uid, SpriteOnOffStateComponent? comp = null){}

    public virtual void ShangeIsOn(EntityUid uid, SpriteOnOffStateComponent? comp = null){}
}
