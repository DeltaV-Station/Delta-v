using Content.Shared.Stray.Plesen.PlesenFloor;
using Content.Shared.Popups;
using Robust.Client.GameObjects;

namespace Content.Client.Stray.Plesen.PlesenFloor;

public sealed class PlesenFloorSystem : SharedPlesenFloorSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void del(EntityUid toDel){
        EntityManager.QueueDeleteEntity(toDel);
    }
    protected override void UpdateVis(EntityUid uid, PlesenFloorComponent? component = null){
        if (!Resolve(uid, ref component) || !TryComp(uid, out SpriteComponent? sprite))
            return;
        _popup.PopupEntity("Выростает", uid, PopupType.MediumCaution);
        sprite.LayerSetVisible(0,true);
    }
}
