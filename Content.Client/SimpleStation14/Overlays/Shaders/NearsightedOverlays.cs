using Content.Shared.SimpleStation14.Traits.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.SimpleStation14.Overlays.Shaders;

public sealed class NearsightedOverlay : Overlay
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    private readonly ShaderInstance _nearsightShader;

    public float Radius;
    private float _oldRadius;
    public float Darkness;
    private float _oldDarkness;

    private float _lerpTime;
    public float LerpDuration;


    public NearsightedOverlay()
    {
        IoCManager.InjectDependencies(this);
        _nearsightShader = _prototypeManager.Index<ShaderPrototype>("GradientCircleMask").InstanceUnique();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        // Check if the player has a NearsightedComponent and is controlling it
        if (!_entityManager.TryGetComponent(_playerManager.LocalPlayer?.ControlledEntity, out NearsightedComponent? nearComp) ||
            _playerManager.LocalPlayer?.ControlledEntity != nearComp.Owner)
            return false;

        // Check if the player has an EyeComponent and if the overlay should be drawn for this eye
        if (!_entityManager.TryGetComponent(_playerManager.LocalPlayer?.ControlledEntity, out EyeComponent? eyeComp) ||
            args.Viewport.Eye != eyeComp.Eye)
            return false;

        return true;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        // We already checked if they have a NearsightedComponent and are controlling it in BeforeDraw, so we assume this hasn't changed
        var nearComp = _entityManager.GetComponent<NearsightedComponent>(_playerManager.LocalPlayer!.ControlledEntity!.Value);

        // Set LerpDuration based on nearComp.LerpDuration
        LerpDuration = nearComp.LerpDuration;

        // Set the radius and darkness values based on whether the player is wearing glasses or not
        if (nearComp.Active)
        {
            Radius = nearComp.EquippedRadius;
            Darkness = nearComp.EquippedAlpha;
        }
        else
        {
            Radius = nearComp.Radius;
            Darkness = nearComp.Alpha;
        }


        var viewport = args.WorldAABB;
        var handle = args.WorldHandle;
        var distance = args.ViewportBounds.Width;

        var lastFrameTime = (float) _timing.FrameTime.TotalSeconds;


        // If the current radius value is different from the previous one, lerp between them
        if (!MathHelper.CloseTo(_oldRadius, Radius, 0.001f))
        {
            _lerpTime += lastFrameTime;
            var t = MathHelper.Clamp(_lerpTime / LerpDuration, 0f, 1f); // Calculate lerp time
            _oldRadius = MathHelper.Lerp(_oldRadius, Radius, t); // Lerp between old and new radius values
        }
        // If the current radius value is the same as the previous one, reset the lerp time and old radius value
        else
        {
            _lerpTime = 0f;
            _oldRadius = Radius;
        }

        // If the current darkness value is different from the previous one, lerp between them
        if (!MathHelper.CloseTo(_oldDarkness, Darkness, 0.001f))
        {
            _lerpTime += lastFrameTime;
            var t = MathHelper.Clamp(_lerpTime / LerpDuration, 0f, 1f); // Calculate lerp time
            _oldDarkness = MathHelper.Lerp(_oldDarkness, Darkness, t); // Lerp between old and new darkness values
        }
        // If the current darkness value is the same as the previous one, reset the lerp time and old darkness value
        else
        {
            _lerpTime = 0f;
            _oldDarkness = Darkness;
        }


        // Calculate the outer and inner radii based on the current radius value
        var outerMaxLevel = 0.6f * distance;
        var outerMinLevel = 0.06f * distance;
        var innerMaxLevel = 0.02f * distance;
        var innerMinLevel = 0.02f * distance;

        var outerRadius = outerMaxLevel - _oldRadius * (outerMaxLevel - outerMinLevel);
        var innerRadius = innerMaxLevel - _oldRadius * (innerMaxLevel - innerMinLevel);

        // Set the shader parameters and draw the overlay
        _nearsightShader.SetParameter("time", 0.0f);
        _nearsightShader.SetParameter("color", new Vector3(1f, 1f, 1f));
        _nearsightShader.SetParameter("darknessAlphaOuter", _oldDarkness);
        _nearsightShader.SetParameter("innerCircleRadius", innerRadius);
        _nearsightShader.SetParameter("innerCircleMaxRadius", innerRadius);
        _nearsightShader.SetParameter("outerCircleRadius", outerRadius);
        _nearsightShader.SetParameter("outerCircleMaxRadius", outerRadius + 0.2f * distance);
        handle.UseShader(_nearsightShader);
        handle.DrawRect(viewport, Color.Black);

        handle.UseShader(null);
    }
}
