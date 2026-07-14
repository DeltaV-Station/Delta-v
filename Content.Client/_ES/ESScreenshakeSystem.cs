using System.Numerics;
using Content.Shared._DV.CCVars;
using Content.Shared._ES.Camera;
using Content.Shared.Camera;
using Robust.Shared.Configuration;
using Robust.Shared.Noise;
using Robust.Shared.Timing;

namespace Content.Client._ES;

// DeltaV - Anything not marked DeltaV is Ephemeral Space code moved from SharedESScreenshakeSystem
public sealed class ESScreenshakeSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!; // DeltaV
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedESScreenshakeSystem _shared = default!;

    private bool _disabled; // DeltaV

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ESScreenshakeComponent, GetEyeOffsetEvent>(OnGetEyeOffset);
        SubscribeLocalEvent<ESScreenshakeComponent, ESGetEyeRotationEvent>(OnGetEyeRotation);

        _config.OnValueChanged(DCCVars.EsScreenshakeDisabled, OnDisabledChanged, true); // DeltaV
    }
    private void OnDisabledChanged(bool obj)
    {
        _disabled = obj;
    }

    private void OnGetEyeOffset(Entity<ESScreenshakeComponent> ent, ref GetEyeOffsetEvent args)
    {
        if (!TryComp<EyeComponent>(ent, out var eye) || _disabled) // DeltaV - check if disabled
            return;

        var noise = new FastNoiseLite(67);
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        var accumulatedOffset = Vector2.Zero;
        var maxOffset = new Vector2(0.15f, 0.15f);
        foreach (var command in ent.Comp.Commands)
        {
            if (command.Translational == null)
                continue;

            var trauma =
                _shared.CalculateTraumaValueForCurrentTime(command.Translational, command.Start);
            if (trauma <= 0)
                continue;

            noise.SetFrequency(command.Translational.Frequency);

            // using the starst c ommand for y pos kinda doesnt work in the case where multiple shakes get sent at the same time
            // and the shakes are identical otherwise. but like dont do that or something idk
            var offsetX = (maxOffset.X * trauma) * noise.GetNoise((float)_timing.RealTime.TotalMilliseconds,
                (float)command.Start.TotalMilliseconds);
            noise.SetSeed(68);
            var offsetY = (maxOffset.Y * trauma) * noise.GetNoise((float)_timing.RealTime.TotalMilliseconds,
                (float)command.Start.TotalMilliseconds);
            noise.SetSeed(67);
            accumulatedOffset += new Vector2(offsetX, offsetY);
        }

        args.Offset += accumulatedOffset;
    }

    private void OnGetEyeRotation(Entity<ESScreenshakeComponent> ent, ref ESGetEyeRotationEvent args)
    {
        if (!TryComp<EyeComponent>(ent, out var eye) || _disabled) // DeltaV - check if disabled
            return;

        var noise = new FastNoiseLite(67 + 420); // Epic bacon
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        // 20deg max
        var accumulatedAngle = Angle.Zero;
        var maxAngleDegrees = 20f;
        foreach (var command in ent.Comp.Commands)
        {
            if (command.Rotational == null)
                continue;

            var trauma =
                _shared.CalculateTraumaValueForCurrentTime(command.Rotational, command.Start);
            if (trauma <= 0)
                continue;

            noise.SetFrequency(command.Rotational.Frequency);

            var angle = (maxAngleDegrees * trauma) * noise.GetNoise((float)_timing.RealTime.TotalMilliseconds, (float)command.Start.TotalMilliseconds);
            accumulatedAngle += Angle.FromDegrees(angle);
        }

        // TODO ughhh this shit breaks with something idk
        args.Rotation += accumulatedAngle;
    }
}
