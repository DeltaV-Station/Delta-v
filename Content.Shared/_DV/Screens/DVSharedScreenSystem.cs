namespace Content.Shared._DV.Screens;

public abstract class DVSharedScreenSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    protected void UpdateVisuals(Entity<DVScreenComponent> ent)
    {
        _appearance.SetData(ent.Owner, DVScreenVisuals.AlertLevel, ent.Comp.AlertLevel ?? string.Empty);
        _appearance.SetData(ent.Owner, DVScreenVisuals.ShowAlertBorder, ent.Comp.ShowAlertBorder);
        _appearance.SetData(ent.Owner, DVScreenVisuals.Content, ent.Comp.Content);
    }
}
