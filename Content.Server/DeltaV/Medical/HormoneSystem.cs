using Content.Shared.Humanoid;
using Content.Shared.DeltaV.Medical;
using Content.Shared.DeltaV.Traits;

namespace Content.Server.DeltaV.Medical;

/// <summary>
///     System to handle hormonal effects
/// </summary>
public sealed class HormoneSystem : EntitySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FeminizedComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<FeminizedComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MasculinizedComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MasculinizedComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(EntityUid uid, IHormoneComponent component, ComponentInit args)
    {
        HumanoidAppearanceComponent? humanoid = null;

        if (!Resolve(uid, ref humanoid) || humanoid == null || humanoid.Sex == component.Target) {
            return;
        }

        if (TryComp<HormoneSensitiveComponent>(uid, out var trait) && trait.Target == component.Target) {
            component.Original = humanoid.Sex;
            _humanoidSystem.SetSex(uid, component.Target);
        }
    }

    private void OnShutdown(EntityUid uid, IHormoneComponent component, ComponentShutdown args)
    {
        HumanoidAppearanceComponent? humanoid = null;

        if (!Resolve(uid, ref humanoid) || humanoid == null || component.Original == null) {
            return;
        }

        _humanoidSystem.SetSex(uid, component.Original.Value);
    }
}
