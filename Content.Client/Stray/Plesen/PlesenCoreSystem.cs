using Content.Shared.Stray.Plesen.PlesenCore;
using Content.Shared.Popups;
using Robust.Client.GameObjects;

namespace Content.Client.Stray.Plesen.PlesenCore;

public sealed class PlesenCoreSystem : SharedPlesenCoreSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void del(EntityUid toDel){
        EntityManager.QueueDeleteEntity(toDel);
    }
    protected override void UpdateVis(EntityUid uid, PlesenCoreComponent? component = null){
        if (!Resolve(uid, ref component) || !TryComp(uid, out SpriteComponent? sprite))
            return;
        _popup.PopupEntity("Выростает", uid, PopupType.LargeCaution);
        sprite.LayerSetVisible(1,true);
    }
}
