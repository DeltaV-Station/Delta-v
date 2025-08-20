using Content.Shared.Examine;

namespace Content.Shared._DV.Access;

/// <summary>
///     Allows an ID card to set the access level of interacted items
/// </summary>

public abstract class SharedUnlockOnAlertLevelSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UnlockOnAlertLevelComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<UnlockOnAlertLevelComponent> ent, ref ExaminedEvent args)
    {
        var levels = ""; // 90% sure there's actually a smartass way to do this with fluent but I'm doing this becasue I fucking love for loops
        for (int i = 0; i < ent.Comp.AlertLevels.Count; i++)
        {
            if (i == ent.Comp.AlertLevels.Count - 2)
                levels += ent.Comp.AlertLevels[i] + ", or ";
            else
            if (i == ent.Comp.AlertLevels.Count - 1)
                levels += ent.Comp.AlertLevels[i];
            else
                levels += ent.Comp.AlertLevels[i] + ", ";
        }
        args.PushText(Loc.GetString("unlock-on-alert-level-examine", ("levels", levels)));
    }
}
