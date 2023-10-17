using Robust.Shared.Audio;

namespace Content.Server.Fugitive
{
    [RegisterComponent]
    public sealed partial class FugitiveComponent : Component
    {
        [DataField("spawnSound")]
        public SoundSpecifier SpawnSoundPath = new SoundPathSpecifier("/Audio/Effects/clang.ogg");

        [DataField("firstMindAdded")]
        public bool FirstMindAdded = false;
    }
}
