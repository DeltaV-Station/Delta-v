using Content.Server.Chat.Systems;

namespace Content.Server.DeltaV.AACTablet;

public sealed class AACTabletSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();
    }
}