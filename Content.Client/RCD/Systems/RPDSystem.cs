// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
// SPDX-License-Identifier: MIT

using Content.Client.Items;
using Content.Client.Message;
using Content.Shared.RCD.Components;
using Content.Shared.RCD.Systems;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.RCD.Systems;

public sealed class RPDSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        Subs.ItemStatus<RCDComponent>(OnItemStatus);
    }

    private Control OnItemStatus(Entity<RCDComponent> entity)
    {
        return new RPDModeStatusControl(entity);
    }

    private sealed class RPDModeStatusControl : Control
    {
        private readonly RichTextLabel _label = new()
        {
            StyleClasses = { "ItemStatus" }
        };

        private readonly EntityUid _uid;
        private readonly bool _isRpd;
        private readonly RCDSystem _rcdSystem;

        public RPDModeStatusControl(Entity<RCDComponent> entity)
        {
            _uid = entity.Owner;
            _isRpd = entity.Comp.IsRpd;
            _rcdSystem = EntitySystem.Get<RCDSystem>();
            AddChild(_label);
        }

        protected override void FrameUpdate(FrameEventArgs args)
        {
            if (!_isRpd) return;

            base.FrameUpdate(args);

            var currentMode = _rcdSystem.GetCurrentRpdMode(_uid);

            var modeKey = $"rcd-rpd-mode-{currentMode.ToString().ToLowerInvariant()}";
            var modeName = Robust.Shared.Localization.Loc.GetString(modeKey);

            _label.SetMarkup(Robust.Shared.Localization.Loc.GetString("rcd-item-status-mode",
                ("mode", $"[color=cyan]{modeName}[/color]")));
        }
    }
}
