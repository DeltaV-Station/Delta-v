using JetBrains.Annotations;
using Content.Shared.Stray.SpriteOnOffState;
using Content.Shared.Popups;

namespace Content.Server.Stray.SpriteOnOffState;


[UsedImplicitly]
public sealed class SpriteOnOffStateSystem : SharedSpriteOnOffStateSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void ShangeIsOn(EntityUid uid, SpriteOnOffStateComponent? comp = null){
        if (!Resolve(uid, ref comp))
            return;

        ChIsOn(uid, !comp.IsOn, comp);

        if(comp.Popup != ""){
            _popup.PopupEntity(comp.Popup, uid);
        }
    }
}
