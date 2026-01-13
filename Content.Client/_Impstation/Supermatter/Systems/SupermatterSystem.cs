using Content.Shared._Impstation.Supermatter.Components;
using Content.Shared._Impstation.Supermatter.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Storage.Components;
using Robust.Client.GameObjects;
namespace Content.Client._Impstation.Supermatter.Systems;

/// <inheritdoc/>
public sealed class SupermatterSystem : SharedSupermatterSystem
{
    /// <summary>
    /// This is used to update the supermatter's glow.
    /// </summary>
    private EntityQuery<PointLightComponent> _lightQuery;
    
    public override void Initialize()
    {
        base.Initialize();

        _lightQuery = GetEntityQuery<PointLightComponent>();
        
        SubscribeLocalEvent<SupermatterComponent, AfterAutoHandleStateEvent>(OnConfigurationState);
    }
    
    private void OnConfigurationState(Entity<SupermatterComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if(AppearanceQuery.TryComp(ent, out var appearance))
        {
            UpdateAppearanceFromState((ent, ent.Comp, appearance));
            UpdateAppearanceFromPsychologicalSoothing((ent, ent.Comp, appearance));
        }
        
        UpdateLinkedPorts(ent);
        UpdateSpeech(ent);
        UpdateAmbient(ent);
        if (_lightQuery.TryComp(ent, out var light))
            UpdateLight(ent, light);
        
    }

    protected override void UpdateSupermatter(Entity<SupermatterComponent> ent)
    {
        // The client doesn't have a lot to do here yet.
    }
    
    protected override void OnSupermatterDelamination(EntityUid uid, SupermatterComponent sm, SupermatterDelaminationEvent args)
    {
        Log.Info("Predicting delamination!");
        
        if (sm.PreferredDelamination is null)
        {
            PredictedQueueDel(uid);
            return;
        }
        
        // Things not able to be predicted:
        // - The Chat/Radio message
        // - The global sound
        // - The gamerules
        
        var xform = Transform(uid);
        var mapId = xform.MapID;
        
        var mobLookup = new HashSet<Entity<MobStateComponent>>();
        EntityLookup.GetEntitiesOnMap(mapId, mobLookup);
        var insideEntityStorageQuery = GetEntityQuery<InsideEntityStorageComponent>();
        
        foreach (var mob in mobLookup)
        {
            if (insideEntityStorageQuery.HasComp(mob)) continue;
            
            Effects.ApplyEffects(mob, sm.PreferredDelamination.MobEffects);
        }

        
        Effects.ApplyEffects(uid, sm.PreferredDelamination.SupermatterEffects);
        
        // Not every delamination will automatically destroy the supermatter.
        // So we're going to queue it for deletion just to be sure.
        PredictedQueueDel(uid);
    }
}
