using Content.Shared.Stray.SpriteOnOffState; // название П.И. (пространства имён) в которую входит основной скрипт системы
using Robust.Client.GameObjects;

namespace Content.Client.Stray.SpriteOnOffState; // название П.И. в которую входит скрипт, в основном это расположение скрипта


public sealed class SpriteOnOffStateSystem : SharedSpriteOnOffStateSystem
{
    protected override void UpdateVisuals(EntityUid uid, SpriteOnOffStateComponent? comp = null){
        if (!Resolve(uid, ref comp) || !TryComp(uid, out SpriteComponent? sprite))
            return;

        var state = comp.IsOn==true?comp.OnState:comp.OffState;
        sprite.LayerSetState(0,$"{state}");
    }
}
