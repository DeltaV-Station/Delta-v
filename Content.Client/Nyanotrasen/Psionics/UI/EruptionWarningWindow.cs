using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Localization;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.Psionics.UI
{
    public sealed class EruptionWarningWindow : DefaultWindow
    {
        public readonly Button AcknowledgeButton;

        public EruptionWarningWindow()
        {

            Title = Loc.GetString("eruption-warning-window-title");

            Contents.AddChild(new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Children =
                {
                    new BoxContainer
                    {
                        Orientation = LayoutOrientation.Vertical,
                        Children =
                        {
                            (new Label()
                            {
                                Text = Loc.GetString("eruption-warning-window-prompt-text-part")
                            }),
                            new BoxContainer
                            {
                                Orientation = LayoutOrientation.Horizontal,
                                Align = AlignMode.Center,
                                Children =
                                {
                                    (AcknowledgeButton = new Button
                                    {
                                        Text = Loc.GetString("eruption-warning-window-acknowledge-button"),
                                    })
                                }
                            },
                        }
                    },
                }
            });
        }
    }
}
