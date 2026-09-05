using Content.Server._DV.Objectives.Eui;
using Content.Server._Impstation.Thaven;
using Content.Shared._Impstation.Thaven.Components;
using Content.Server.Objectives;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Server.Administration.Systems;

public sealed partial class AdminVerbSystem : EntitySystem
{
    [Dependency] private readonly ThavenMoodsSystem _moods = default!;
    private void AddDVAdminVerbs(GetVerbsEvent<Verb> args)
    {
        if (TryComp<ThavenMoodsComponent>(args.Target, out var moods))
        {
            args.Verbs.Add(new Verb()
            {
                Text = Loc.GetString("thaven-moods-ui-verb"),
                Category = VerbCategory.Admin,
                Act = () =>
                {
                    var ui = new ThavenMoodsEui(_moods, EntityManager, _adminManager);
                    if (!_playerManager.TryGetSessionByEntity(args.User, out var session))
                        return;

                    _euiManager.OpenEui(ui, session);
                    ui.UpdateMoods(moods, args.Target);
                },
                Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Interface/Actions/actions_borg.rsi"), "state-laws"),
            });
        }


        if (_mindSystem.TryGetMind(args.Target, out var mindId, out var mindComp) &&
            mindComp.UserId != null)
        {
            args.Verbs.Add(new Verb()
            {
                Text = Loc.GetString("Edit Objectives"),
                Category = VerbCategory.Admin,
                Act = () =>
                {
                    if (!_playerManager.TryGetSessionByEntity(args.User, out var session))
                        return;

                    var ui = new ObjectiveEditorEui();
                    _euiManager.OpenEui(ui, session);
                    ui.UpdateObjectivesFor((mindId, mindComp));
                },
                Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Interface/Actions/actions_borg.rsi"), "state-laws"),
            });
        }
    }
}
