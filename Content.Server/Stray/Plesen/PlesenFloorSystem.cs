using Content.Shared.Stray.Plesen.PlesenFloor;
using JetBrains.Annotations;

namespace Content.Server.Stray.Plesen.PlesenFloor;

[UsedImplicitly]
public sealed class PlesenFloorSystem : SharedPlesenFloorSystem
{
    public override void del(EntityUid toDel){
        EntityManager.QueueDeleteEntity(toDel);
    }
}
