// SPDX-FileCopyrightText: 2025 Coenx-flex
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.UserInterface.Controls;
using Content.Shared._Mono.CorticalBorer;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Mono.CorticalBorer
{
    [UsedImplicitly]
    public sealed class CorticalBorerDispenserBoundUserInterface : BoundUserInterface
    {
        private CorticalBorerDispenserWindow? _window;

        public CorticalBorerDispenserBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {

        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<CorticalBorerDispenserWindow>();
            _window.SetInfoFromEntity(EntMan, Owner);

            // Setup static button actions.
            _window.AmountGrid.OnButtonPressed += s => SendMessage(new CorticalBorerDispenserSetInjectAmountMessage(s));

            _window.OnDispenseReagentButtonPressed += id => SendMessage(new CorticalBorerDispenserInjectMessage(id));
        }

        /// <summary>
        /// Update the UI each time new state data is sent from the server.
        /// </summary>
        /// <param name="state">
        /// Data of the <see cref="ReagentDispenserComponent"/> that this UI represents.
        /// Sent from the server.
        /// </param>
        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            var castState = (CorticalBorerDispenserBoundUserInterfaceState) state;
            _window?.UpdateState(castState);
        }
    }
}
