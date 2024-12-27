using Content.Shared.DeltaV.Hologram;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client.DeltaV.Hologram;

public sealed class HologramSystem : SharedHologramSystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly OccluderSystem _occluder = default!;
    [Dependency] private readonly EntityManager _entMan = default!;

    private ShaderInstance _shader = default!;

    public override void Initialize()
    {
        base.Initialize();

        _shader = _protoMan.Index<ShaderPrototype>("HologramDeltaV").InstanceUnique();
        SubscribeLocalEvent<HologramComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<HologramComponent, ComponentStartup>(OnStartup);
    }

    private void SetShader(EntityUid uid, bool enabled, HologramComponent? component = null, SpriteComponent? sprite = null)
    {
        if (!Resolve(uid, ref component, ref sprite, false))
            return;

        sprite.PostShader = enabled ? _shader : null;
    }

    private void OnStartup(EntityUid uid, HologramComponent component, ComponentStartup args)
    {
        SetShader(uid, true, component);

        component.Occludes = _entMan.TryGetComponent<OccluderComponent>(uid, out var occluder) && occluder.Enabled;
        if (component.Occludes)
            _occluder.SetEnabled(uid, false);
    }

    private void OnShutdown(EntityUid uid, HologramComponent component, ComponentShutdown args)
    {
        SetShader(uid, false, component);
        if (component.Occludes)
            _occluder.SetEnabled(uid, true);
    }
}
