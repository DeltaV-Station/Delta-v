using Content.Server._DV.HealthAnalyzerPlus.Components;
using Content.Server.Medical.Components;
using Content.Shared.CartridgeLoader;

namespace Content.Server._DV.CartridgeLoader.Cartridges;

public sealed class MedTekPlusCartridgeSystem : EntitySystem
{
    // We need to remove the normal HealthAnalyzerComponent when we add HealthAnalyzerPlus
    // so they don't fight. Keep track of if we had it or not so we can add it back if we
    // remove HealthAnalyzerPlus
    private bool _hadHealthAnalyzerComponent;

    public override void Initialize()
    {
        base.Initialize();

        _hadHealthAnalyzerComponent = false;

        SubscribeLocalEvent<MedTekPlusCartridgeComponent, CartridgeAddedEvent>(OnCartridgeAdded);
        SubscribeLocalEvent<MedTekPlusCartridgeComponent, CartridgeRemovedEvent>(OnCartridgeRemoved);
    }

    private void OnCartridgeAdded(Entity<MedTekPlusCartridgeComponent> ent, ref CartridgeAddedEvent args)
    {
        var healthAnalyzer = EnsureComp<HealthAnalyzerPlusComponent>(args.Loader);

        if ( HasComp<HealthAnalyzerComponent>( args.Loader ) )
        {
            _hadHealthAnalyzerComponent = true;
            RemComp<HealthAnalyzerComponent>( args.Loader );
        }
    }

    private void OnCartridgeRemoved(Entity<MedTekPlusCartridgeComponent> ent, ref CartridgeRemovedEvent args)
    {
        // only remove when the program itself is removed
        //if (!_cartridgeLoaderSystem.HasProgram<MedTekPlusCartridgeComponent>(args.Loader))
        //{
            RemComp<HealthAnalyzerPlusComponent>(args.Loader);

            if ( _hadHealthAnalyzerComponent )
            {
                EnsureComp<HealthAnalyzerComponent>( args.Loader );
            }
        //}
    }
}
