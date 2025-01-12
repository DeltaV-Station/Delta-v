namespace Content.Server._DV.Chemistry.Components;

/// <summary>
/// For some reason if you set HyposprayComponent onlyAffectsMobs to true it would be able to draw from containers
/// even if injectOnly is also true. I don't want to modify HypospraySystem, so I made this component.
/// - Original Goob Author Aviu00
/// </summary>
[RegisterComponent]
public sealed partial class HyposprayBlockNonMobInjectionComponent : Component
{
}
