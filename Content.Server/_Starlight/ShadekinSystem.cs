using Content.Shared.Humanoid;
using Content.Shared.Alert;
using Robust.Server.GameObjects;
using Content.Shared.Examine;
using Robust.Server.Containers;
using Content.Shared._Starlight;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Damage;
using Content.Server.Chat.Managers;
using Content.Shared.Damage.Systems;
using Content.Shared._Goobstation.Overlays;
using Robust.Shared.Timing;
using Content.Server.Body.Components;


namespace Content.Server._Starlight;

public sealed class ShadekinSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private ContainerSystem _container = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    private sealed class LightCone
    {
        public float Direction { get; set; }
        public float InnerWidth { get; set; }
        public float OuterWidth { get; set; }
    }
    private readonly Dictionary<string, List<LightCone>> lightMasks = new()
    {
        ["/Textures/Effects/LightMasks/cone.png"] = new List<LightCone>
    {
        new LightCone { Direction = 0, InnerWidth = 30, OuterWidth = 60 }
    },
        ["/Textures/Effects/LightMasks/double_cone.png"] = new List<LightCone>
    {
        new LightCone { Direction = 0, InnerWidth = 30, OuterWidth = 60 },
        new LightCone { Direction = 180, InnerWidth = 30, OuterWidth = 60 }
    }
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadekinComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<ShadekinComponent, EyeColorInitEvent>(OnEyeColorChange);
        SubscribeLocalEvent<ShadekinComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeedModifiers);
    }

    private void OnInit(EntityUid uid, ShadekinComponent component, ComponentStartup args)
    {
        UpdateAlert(uid, component, (short)component.CurrentState);
        RemComp<RespiratorComponent>(uid);
    }

    private void OnEyeColorChange(EntityUid uid, ShadekinComponent component, EyeColorInitEvent args)
    {
        if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
            return;

        humanoid.EyeColor = Color.Black;
        Dirty(uid, humanoid);
    }

    public void UpdateAlert(EntityUid uid, ShadekinComponent component, short state)
    {
        _alerts.ShowAlert(uid, component.ShadekinAlert, state);
    }

    private Angle GetAngle(EntityUid lightUid, SharedPointLightComponent lightComp, EntityUid targetUid)
    {
        var (lightPos, lightRot) = _transform.GetWorldPositionRotation(lightUid);
        lightPos += lightRot.RotateVec(lightComp.Offset);

        var (targetPos, targetRot) = _transform.GetWorldPositionRotation(targetUid);

        var mapDiff = targetPos - lightPos;

        var oppositeMapDiff = (-lightRot).RotateVec(mapDiff);
        var angle = oppositeMapDiff.ToWorldAngle();

        if (angle == double.NaN && _transform.ContainsEntity(targetUid, lightUid) || _transform.ContainsEntity(lightUid, targetUid))
        {
            angle = 0f;
        }

        return angle;
    }

    /// <summary>
    /// Return an illumination float value with is how many "energy" of light is hitting our ent.
    /// WARNING: This function might be expensive, Avoid calling it too much and CACHE THE RESULT!
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public float GetLightExposure(EntityUid uid)
    {
        var illumination = 0f;

        var lightQuery = _lookup.GetEntitiesInRange<PointLightComponent>(Transform(uid).Coordinates, 20, LookupFlags.Uncontained);

        foreach (var light in lightQuery)
        {
            if (!light.Comp.Enabled
                || light.Comp.Radius < 1
                || light.Comp.Energy <= 0)
                continue;

            var (lightPos, lightRot) = _transform.GetWorldPositionRotation(light);
            lightPos += lightRot.RotateVec(light.Comp.Offset);

            if (!_examine.InRangeUnOccluded(light, uid, light.Comp.Radius, null))
                continue;

            Transform(uid).Coordinates.TryDistance(EntityManager, Transform(light).Coordinates, out var dist);

            var denom = dist / light.Comp.Radius;
            var attenuation = 1 - (denom * denom);
            var calculatedLight = 0f;

            if (light.Comp.MaskPath is not null)
            {
                var angleToTarget = GetAngle(light, light.Comp, uid);
                foreach (var cone in lightMasks[light.Comp.MaskPath])
                {
                    var coneLight = 0f;
                    var angleAttenuation = (float)Math.Min((float)Math.Max(cone.OuterWidth - angleToTarget, 0f), cone.InnerWidth) / cone.OuterWidth;

                    if (angleToTarget.Degrees - cone.Direction > cone.OuterWidth)
                        continue;
                    else if (angleToTarget.Degrees - cone.Direction > cone.InnerWidth
                        && angleToTarget.Degrees - cone.Direction < cone.OuterWidth)
                        coneLight = light.Comp.Energy * attenuation * attenuation * angleAttenuation;
                    else
                        coneLight = light.Comp.Energy * attenuation * attenuation;

                    calculatedLight = Math.Max(calculatedLight, coneLight);
                }
            }
            else
                calculatedLight = light.Comp.Energy * attenuation * attenuation;

            illumination += calculatedLight; //Math.Max(illumination, calculatedLight);
        }

        return illumination;
    }

    private void SetPassiveBuff(EntityUid uid, ShadekinState state)
    {
        if (!TryComp<PassiveDamageComponent>(uid, out var passive))
            return;

        if (state == ShadekinState.Extreme || state == ShadekinState.Annoying || state == ShadekinState.High)
        {
            passive.DamageCap = 1;
        }
        else if (state == ShadekinState.Low)
        {
            passive.DamageCap = 20;
            passive.AllowedStates.Clear();
            passive.AllowedStates.Add(MobState.Alive);
            passive.Interval = 1f;
        }
        else if (state != ShadekinState.Dark)
        {
            passive.DamageCap = 0;
            passive.AllowedStates.Clear();
            passive.AllowedStates.Add(MobState.Alive);
            passive.AllowedStates.Add(MobState.Critical);
            passive.AllowedStates.Add(MobState.Dead);
            passive.Interval = 0.5f;
        }
    }

    private void ToggleNightVision(EntityUid uid, ShadekinState state)
    {
        if (state == ShadekinState.Dark)
            EnsureComp<NightVisionComponent>(uid);
        else
            RemComp<NightVisionComponent>(uid);
    }

    private void ApplyLightDamage(EntityUid uid, ShadekinState state)
    {
        if (state != ShadekinState.Extreme)
            return;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Heat", 5);
        _damageable.TryChangeDamage(uid, damage, true, false);

    }

    private void OnRefreshMovementSpeedModifiers(EntityUid uid, ShadekinComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (component.CurrentState == ShadekinState.Low || component.CurrentState == ShadekinState.Annoying ||
                component.CurrentState == ShadekinState.Dark || component.CurrentState == ShadekinState.Invalid)
            return;

        if (!TryComp<MovementSpeedModifierComponent>(uid, out var movement))
            return;

        var sprintDif = movement.BaseWalkSpeed / movement.BaseSprintSpeed;
        args.ModifySpeed(1f, sprintDif);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShadekinComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (_timing.CurTime < component.NextUpdate)
                continue;

            component.NextUpdate = _timing.CurTime + component.UpdateCooldown;

            var lightExposure = 0f;

            if (!_container.IsEntityInContainer(uid))
                lightExposure = GetLightExposure(uid);

            if (lightExposure >= 15f)
                component.CurrentState = ShadekinState.Extreme;
            else if (lightExposure >= 10f)
                component.CurrentState = ShadekinState.High;
            else if (lightExposure >= 5f)
                component.CurrentState = ShadekinState.Annoying;
            else if (lightExposure >= 0.8f)
                component.CurrentState = ShadekinState.Low;
            else
                component.CurrentState = ShadekinState.Dark;

            UpdateAlert(uid, component, (short)component.CurrentState);

            SetPassiveBuff(uid, component.CurrentState);
            ToggleNightVision(uid, component.CurrentState);
            ApplyLightDamage(uid, component.CurrentState);
            _speed.RefreshMovementSpeedModifiers(uid);

            if (component.CurrentState == ShadekinState.Extreme)
                ApplyLightDamage(uid, component.CurrentState);
        }
    }
}
