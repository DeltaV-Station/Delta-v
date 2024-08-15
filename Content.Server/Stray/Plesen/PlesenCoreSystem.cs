using Content.Shared.Stray.Plesen.PlesenCore;
using JetBrains.Annotations;
using Content.Shared.Damage;

namespace Content.Server.Stray.Plesen.PlesenCore;

[UsedImplicitly]
public sealed class PlesenCoreSystem : SharedPlesenCoreSystem
{
    //[Dependency] private readonly DamageableSystem _damageable = default!;
    public override void del(EntityUid toDel){
        EntityManager.QueueDeleteEntity(toDel);
    }
}
