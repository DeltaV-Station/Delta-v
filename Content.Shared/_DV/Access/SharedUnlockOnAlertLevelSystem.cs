using Content.Shared.Examine;
using System.Text;

namespace Content.Shared._DV.Access;

/// <summary>
///     Hanldes the examination of an entity with a UnlockOnAlertLevelComponent
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
        var levels = new StringBuilder();
        for (int i = 0; i < ent.Comp.AlertLevels.Count; i++)
        {
            if (i == ent.Comp.AlertLevels.Count - 2)
                levels.Append(ent.Comp.AlertLevels[i] + ", or ");
            else
            if (i == ent.Comp.AlertLevels.Count - 1)
                levels.Append(ent.Comp.AlertLevels[i]);
            else
                levels.Append(ent.Comp.AlertLevels[i] + ", ");
        }
        args.PushText(Loc.GetString("unlock-on-alert-level-examine", ("levels", levels.ToString())));
    }
}
