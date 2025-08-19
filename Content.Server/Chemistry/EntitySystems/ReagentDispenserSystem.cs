using Content.Server.Chemistry.Components;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Dispenser;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.FixedPoint;
using Content.Shared.Nutrition.EntitySystems;
using JetBrains.Annotations;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Content.Shared.Labels.Components;

namespace Content.Server.Chemistry.EntitySystems
{
    /// <summary>
    /// Contains all the server-side logic for reagent dispensers.
    /// <seealso cref="ReagentDispenserComponent"/>
    /// </summary>
    [UsedImplicitly]
    public sealed class ReagentDispenserSystem : EntitySystem
    {
        [Dependency] private readonly AudioSystem _audioSystem = default!;
        [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
        [Dependency] private readonly SolutionTransferSystem _solutionTransferSystem = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly OpenableSystem _openable = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ReagentDispenserComponent, ComponentStartup>(SubscribeUpdateUiState);
            SubscribeLocalEvent<ReagentDispenserComponent, SolutionContainerChangedEvent>(SubscribeUpdateUiState);
            SubscribeLocalEvent<ReagentDispenserComponent, EntInsertedIntoContainerMessage>(SubscribeUpdateUiState);
            SubscribeLocalEvent<ReagentDispenserComponent, EntRemovedFromContainerMessage>(SubscribeUpdateUiState);
            SubscribeLocalEvent<ReagentDispenserComponent, BoundUIOpenedEvent>(SubscribeUpdateUiState);

            SubscribeLocalEvent<ReagentDispenserComponent, ReagentDispenserSetDispenseAmountMessage>(OnSetDispenseAmountMessage);
            SubscribeLocalEvent<ReagentDispenserComponent, ReagentDispenserDispenseReagentMessage>(OnDispenseReagentMessage);
            SubscribeLocalEvent<ReagentDispenserComponent, ReagentDispenserClearContainerSolutionMessage>(OnClearContainerSolutionMessage);

            SubscribeLocalEvent<ReagentDispenserComponent, MapInitEvent>(OnMapInit, before: new[] { typeof(ItemSlotsSystem) });
        }

        private void SubscribeUpdateUiState<T>(Entity<ReagentDispenserComponent> ent, ref T ev)
        {
            UpdateUiState(ent);
        }

        private void UpdateUiState(Entity<ReagentDispenserComponent> reagentDispenser)
        {
            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedReagentDispenser.OutputSlotName);
            var outputContainerInfo = BuildOutputContainerInfo(outputContainer);

            var inventory = GetInventory(reagentDispenser);

            var state = new ReagentDispenserBoundUserInterfaceState(outputContainerInfo, GetNetEntity(outputContainer), inventory, reagentDispenser.Comp.DispenseAmount);
            _userInterfaceSystem.SetUiState(reagentDispenser.Owner, ReagentDispenserUiKey.Key, state);
        }

        private ContainerInfo? BuildOutputContainerInfo(EntityUid? container)
        {
            if (container is not { Valid: true })
                return null;

            if (_solutionContainerSystem.TryGetFitsInDispenser(container.Value, out _, out var solution))
            {
                return new ContainerInfo(Name(container.Value), solution.Volume, solution.MaxVolume)
                {
                    Reagents = solution.Contents
                };
            }

            return null;
        }

        private List<ReagentInventoryItem> GetInventory(Entity<ReagentDispenserComponent> reagentDispenser)
        {
            var inventory = new List<ReagentInventoryItem>();

            for (var i = 0; i < reagentDispenser.Comp.NumSlots; i++)
            {
                var storageSlotId = ReagentDispenserComponent.BaseStorageSlotId + i;
                var storedContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser.Owner, storageSlotId);

                // Set label from manually-applied label, or metadata if unavailable
                string reagentLabel;
                if (TryComp<LabelComponent>(storedContainer, out var label) && !string.IsNullOrEmpty(label.CurrentLabel))
                    reagentLabel = label.CurrentLabel;
                else if (storedContainer != null)
                    reagentLabel = Name(storedContainer.Value);
                else
                    continue;

                // Get volume remaining and color of solution
                FixedPoint2 quantity = 0f;
                var reagentColor = Color.White;
                if (storedContainer != null && _solutionContainerSystem.TryGetDrainableSolution(storedContainer.Value, out _, out var sol))
                {
                    quantity = sol.Volume;
                    reagentColor = sol.GetColor(_prototypeManager);
                }

                inventory.Add(new ReagentInventoryItem(storageSlotId, reagentLabel, quantity, reagentColor));
            }

            return inventory;
        }

        private void OnSetDispenseAmountMessage(Entity<ReagentDispenserComponent> reagentDispenser, ref ReagentDispenserSetDispenseAmountMessage message)
        {
            reagentDispenser.Comp.DispenseAmount = message.ReagentDispenserDispenseAmount;
            UpdateUiState(reagentDispenser);
            ClickSound(reagentDispenser);
        }

        private void OnDispenseReagentMessage(Entity<ReagentDispenserComponent> reagentDispenser, ref ReagentDispenserDispenseReagentMessage message)
        {
            // Ensure that the reagent is something this reagent dispenser can dispense.
<<<<<<< HEAD
            var storedContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, message.SlotId);
            if (storedContainer == null)
=======
            var storageLocation = message.StorageLocation;
            var storedContainer = storage.StoredItems.FirstOrDefault(kvp => kvp.Value == storageLocation).Key;
            if (storedContainer == EntityUid.Invalid)
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
                return;

            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedReagentDispenser.OutputSlotName);
            if (outputContainer is not { Valid: true } || !_solutionContainerSystem.TryGetFitsInDispenser(outputContainer.Value, out var solution, out _))
                return;

            if (_solutionContainerSystem.TryGetDrainableSolution(storedContainer.Value, out var src, out _) &&
                _solutionContainerSystem.TryGetRefillableSolution(outputContainer.Value, out var dst, out _))
            {
                // force open container, if applicable, to avoid confusing people on why it doesn't dispense
                _openable.SetOpen(storedContainer.Value, true);
                _solutionTransferSystem.Transfer(reagentDispenser,
                        storedContainer.Value, src.Value,
                        outputContainer.Value, dst.Value,
                        (int)reagentDispenser.Comp.DispenseAmount);
            }

            UpdateUiState(reagentDispenser);
            ClickSound(reagentDispenser);
        }

<<<<<<< HEAD
=======
        private void OnEjectReagentMessage(Entity<ReagentDispenserComponent> reagentDispenser, ref ReagentDispenserEjectContainerMessage message)
        {
            if (!TryComp<StorageComponent>(reagentDispenser.Owner, out var storage))
            {
                return;
            }

            var storageLocation = message.StorageLocation;
            var storedContainer = storage.StoredItems.FirstOrDefault(kvp => kvp.Value == storageLocation).Key;
            if (storedContainer == EntityUid.Invalid)
                return;

            _handsSystem.TryPickupAnyHand(message.Actor, storedContainer);
        }

>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        private void OnClearContainerSolutionMessage(Entity<ReagentDispenserComponent> reagentDispenser, ref ReagentDispenserClearContainerSolutionMessage message)
        {
            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedReagentDispenser.OutputSlotName);
            if (outputContainer is not { Valid: true } || !_solutionContainerSystem.TryGetFitsInDispenser(outputContainer.Value, out var solution, out _))
                return;

            _solutionContainerSystem.RemoveAllSolution(solution.Value);
            UpdateUiState(reagentDispenser);
            ClickSound(reagentDispenser);
        }

        private void ClickSound(Entity<ReagentDispenserComponent> reagentDispenser)
        {
            _audioSystem.PlayPvs(reagentDispenser.Comp.ClickSound, reagentDispenser, AudioParams.Default.WithVolume(-2f));
        }

        /// <summary>
        /// Automatically generate storage slots for all NumSlots, and fill them with their initial chemicals.
        /// The actual spawning of entities happens in ItemSlotsSystem's MapInit.
        /// </summary>
        private void OnMapInit(EntityUid uid, ReagentDispenserComponent component, MapInitEvent args)
        {
            // Get list of pre-loaded containers
            List<string> preLoad = new List<string>();
            if (component.PackPrototypeId is not null
                && _prototypeManager.TryIndex(component.PackPrototypeId, out ReagentDispenserInventoryPrototype? packPrototype))
            {
                preLoad.AddRange(packPrototype.Inventory);
            }

            // Populate storage slots with base storage slot whitelist
            for (var i = 0; i < component.NumSlots; i++)
            {
                var storageSlotId = ReagentDispenserComponent.BaseStorageSlotId + i;
                ItemSlot storageComponent = new();
                storageComponent.Whitelist = component.StorageWhitelist;
                storageComponent.Swap = false;
                storageComponent.EjectOnBreak = true;

                // Check corresponding index in pre-loaded container (if exists) and set starting item
                if (i < preLoad.Count)
                    storageComponent.StartingItem = preLoad[i];

                component.StorageSlotIds.Add(storageSlotId);
                component.StorageSlots.Add(storageComponent);
                component.StorageSlots[i].Name = "Storage Slot " + (i+1);
                _itemSlotsSystem.AddItemSlot(uid, component.StorageSlotIds[i], component.StorageSlots[i]);
            }

            _itemSlotsSystem.AddItemSlot(uid, SharedReagentDispenser.OutputSlotName, component.BeakerSlot);
        }
    }
}
