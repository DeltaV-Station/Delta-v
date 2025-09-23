using Content.Server.Light.Components;
using Content.Shared._DV.Light;
using Content.Shared.Light.Components;

namespace Content.Server.Light.EntitySystems;

public sealed partial class ExpendableLightSystem
{
    private void GetLightEnergy(Entity<ExpendableLightComponent> ent, ref OnGetLightEnergyEvent args)
    {
        // This isn't a perfect clone of the Clientside code, as it relies on animation behaviours
        // and thus is by nature different per-client (Especially those with random flickers)
        // This same code is not used clientside, because the client doesn't actually have access to the start time(??)
        float lightFactor;
        switch (ent.Comp.CurrentState)
        {
            case ExpendableLightState.Lit:
                float timeElapsed = ent.Comp.GlowDuration.Seconds - ent.Comp.StateExpiryTime;
                lightFactor = MathF.Min(1.0f, 1.0f - timeElapsed / ent.Comp.FadeInDuration);
                break;
            case ExpendableLightState.Fading:
                lightFactor = MathF.Min(1.0f, ent.Comp.StateExpiryTime / ent.Comp.FadeOutDuration.Seconds);
                break;
            default: // Other states aren't lit.
                return;
        }
        args.LightEnergy = ent.Comp.LitEnergy * lightFactor;
        args.LightRadius = ent.Comp.LitRadius * lightFactor;
    }
}