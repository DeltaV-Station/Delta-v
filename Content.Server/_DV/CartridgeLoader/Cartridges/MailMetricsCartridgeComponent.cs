namespace Content.Server._DV.CartridgeLoader.Cartridges;

/// <summary>
/// Only used for tracking the MailMetrics PDAs.
/// </summary>
[RegisterComponent, Access(typeof(MailMetricsCartridgeSystem))]
public sealed partial class MailMetricsCartridgeComponent : Component;
