using System.Numerics;
using Content.Shared._DV.Vision.Components;
using Content.Shared._Impstation.Supermatter.Components;
using Content.Shared.Audio;
using Content.Shared.Speech;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
namespace Content.Shared._Impstation.Supermatter.Systems;

public abstract partial class SharedSupermatterSystem
{
    [Dependency] protected readonly SharedAmbientSoundSystem Ambient = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] protected readonly SharedPointLightSystem Light = default!;

    /// <summary>
    /// This is used for ambient sounds.
    /// </summary>
    protected EntityQuery<AmbientSoundComponent> AmbientQuery;

    /// <summary>
    /// Controls the appearance of the supermatter.
    /// </summary>
    protected EntityQuery<AppearanceComponent> AppearanceQuery;

    /// <summary>
    /// This is used for speech sounds
    /// </summary>
    protected EntityQuery<SpeechComponent> SpeechQuery;

    /// <summary>
    /// Initializes the system's cosmetic-related properties.
    /// </summary>
    protected virtual void InitializeCosmetic()
    {
        AmbientQuery = GetEntityQuery<AmbientSoundComponent>();
        AppearanceQuery = GetEntityQuery<AppearanceComponent>();
        SpeechQuery = GetEntityQuery<SpeechComponent>();
        
        SubscribeLocalEvent<SupermatterComponent, PsychologicalSoothingChanged>(OnPsychologicalSoothingChanged);
    }

    /// <summary>
    /// Plays normal/delam sounds at a rate determined by power and damage
    /// </summary>
    protected void UpdateAccent(Entity<SupermatterComponent> ent)
    {
        if (ent.Comp.AccentLastTime >= Timing.CurTime || !Random.Prob(0.05f))
            return;

        var aggression = Math.Min((ent.Comp.Damage / 800) * (ent.Comp.Power / 2500), 1) * 100;
        var nextSound = Math.Max(Math.Round((100 - aggression) * 5), ent.Comp.AccentMinCooldown);
        var sound = ent.Comp.CalmAccent;

        if (ent.Comp.AccentLastTime + TimeSpan.FromSeconds(nextSound) > Timing.CurTime)
            return;

        if (ent.Comp.Status >= SupermatterStatusType.Danger)
            sound = ent.Comp.DelamAccent;

        ent.Comp.AccentLastTime = Timing.CurTime;
        DirtyField(ent, ent.Comp, nameof(SupermatterComponent.AccentLastTime));
        Audio.PlayPvs(sound, Transform(ent).Coordinates);
    }

    protected void UpdateAmbient(Entity<SupermatterComponent> ent)
    {
        if (!AmbientQuery.TryComp(ent, out var ambient))
            return;

        var volume = (float) Math.Round(Math.Clamp(ent.Comp.Power / 50 - 5, -5, 5));

        Ambient.SetVolume(ent, volume);

        switch (ent.Comp.Status)
        {
            case >= SupermatterStatusType.Danger when ambient.Sound != ent.Comp.DelamLoopSound:
                Ambient.SetSound(ent, ent.Comp.DelamLoopSound, ambient);
                break;
            case < SupermatterStatusType.Danger when ambient.Sound != ent.Comp.CalmLoopSound:
                Ambient.SetSound(ent, ent.Comp.CalmLoopSound, ambient);
                break;
        }
    }

    protected void UpdateAppearanceFromState(Entity<SupermatterComponent, AppearanceComponent?> ent)
    {
        if (!AppearanceQuery.Resolve(ent, ref ent.Comp2, logMissing: false))
            return;

        var visual = SupermatterCrystalState.Normal;
        if (ent.Comp1.Damage > 0 && ent.Comp1.Damage > ent.Comp1.DamageArchived) // Damaged and not healing
        {
            visual = ent.Comp1.Status switch
            {
                SupermatterStatusType.Delaminating => SupermatterCrystalState.GlowDelam,
                >= SupermatterStatusType.Emergency => SupermatterCrystalState.GlowEmergency,
                _ => SupermatterCrystalState.Glow
            };
        }

        Appearance.SetData(ent, SupermatterVisuals.Crystal, visual, ent.Comp2);
    }

    protected void UpdateAppearanceFromPsychologicalSoothing(Entity<SupermatterComponent, AppearanceComponent?> ent)
    {
        if (!AppearanceQuery.Resolve(ent, ref ent.Comp2, logMissing: false))
            return;

        if (PsyReceiversQuery.TryComp(ent, out var psyReceiver))
        {
            Appearance.SetData(ent, SupermatterVisuals.Psy, psyReceiver.SoothedCurrent, ent.Comp2);
        }
        else if (Appearance.TryGetData(ent, SupermatterVisuals.Psy, out _))
        {
            Appearance.RemoveData(ent, SupermatterVisuals.Psy, ent.Comp2);
        }
    }

    /// <summary>
    /// Scales the energy and radius of the supermatter's light based on its power,
    /// and gradients the color based on its integrity
    /// </summary>
    protected void UpdateLight(Entity<SupermatterComponent> ent, SharedPointLightComponent light)
    {
        // Max light scaling reached at 2500 power
        var scalar = Math.Clamp(ent.Comp.Power / 2500f + 1f, 1f, 2f);

        // Blend colors between hsvNormal at 100% integrity, and hsvDelam at 0% integrity
        var integrity = GetIntegrity(ent.AsNullable());
        var hsvNormal = Color.ToHsv(ent.Comp.LightColorNormal);
        var hsvDelam = Color.ToHsv(ent.Comp.LightColorDelam);
        var hsvFinal = Vector4.Lerp(hsvDelam, hsvNormal, integrity / 100f);

        Light.SetEnergy(ent, 2f * scalar, light);
        Light.SetRadius(ent, 10f * scalar, light);
        Light.SetColor(ent, Color.FromHsv(hsvFinal), light);
    }

    protected void UpdateSpeech(Entity<SupermatterComponent> ent)
    {
        if (!SpeechQuery.TryComp(ent, out var speech))
            return;

        speech.SpeechSounds = ent.Comp.Status switch
        {
            < SupermatterStatusType.Delaminating when ent.Comp.Damage < ent.Comp.DamageArchived => ent.Comp.StatusSilentSound,

            SupermatterStatusType.Warning => ent.Comp.StatusWarningSound,
            SupermatterStatusType.Danger => ent.Comp.StatusDangerSound,
            SupermatterStatusType.Emergency => ent.Comp.StatusEmergencySound,
            SupermatterStatusType.Delaminating => ent.Comp.StatusDelamSound,

            _ => ent.Comp.StatusSilentSound
        };
    }

    private void OnPsychologicalSoothingChanged(Entity<SupermatterComponent> ent, ref PsychologicalSoothingChanged args)
    {
        UpdateAppearanceFromPsychologicalSoothing(ent);
    }
}
