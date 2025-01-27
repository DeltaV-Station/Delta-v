using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry
{
    /// <summary>
    /// This class holds constants that are shared between client and server.
    /// </summary>
    public sealed class SharedChemMaster
    {
        public const uint PillTypes = 20;
        public const string BufferSolutionName = "buffer";
        public const string PillBufferSolutionName = "pillBuffer";
        public const string InputSlotName = "beakerSlot";
        public const string OutputSlotName = "outputSlot";
        public const string PillSolutionName = "food";
        public const string BottleSolutionName = "drink";
        public const uint LabelMaxLength = 50;
    }

    [Serializable, NetSerializable]
    public sealed class ChemMasterSetPillTypeMessage : BoundUserInterfaceMessage
    {
        public readonly uint PillType;

        public ChemMasterSetPillTypeMessage(uint pillType)
        {
            PillType = pillType;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChemMasterReagentAmountButtonMessage : BoundUserInterfaceMessage
    {
        public readonly ReagentId ReagentId;
        public readonly int Amount;
        public readonly bool FromBuffer;
        public readonly bool IsOutput;

        public ChemMasterReagentAmountButtonMessage(ReagentId reagentId, int amount, bool fromBuffer, bool isOutput)
        {
            ReagentId = reagentId;
            Amount = amount;
            FromBuffer = fromBuffer;
            IsOutput = isOutput;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChemMasterCreatePillsMessage : BoundUserInterfaceMessage
    {
        public readonly uint Dosage;
        public readonly uint Number;
        public readonly string Label;

        public ChemMasterCreatePillsMessage(uint dosage, uint number, string label)
        {
            Dosage = dosage;
            Number = number;
            Label = label;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChemMasterOutputToBottleMessage : BoundUserInterfaceMessage
    {
        public readonly uint Dosage;
        public readonly string Label;

        public ChemMasterOutputToBottleMessage(uint dosage, string label)
        {
            Dosage = dosage;
            Label = label;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChemMasterSortMethodUpdated(int sortMethod) : BoundUserInterfaceMessage
    {
        public readonly int SortMethod = sortMethod;
    }

    [Serializable, NetSerializable]
    public sealed class ChemMasterTransferringAmountUpdated(int transferringAmount) : BoundUserInterfaceMessage
    {
        public readonly int TransferringAmount = transferringAmount;
    }

    public enum ChemMasterMode
    {
        Transfer,
        Discard,
    }

    /// <summary>
    /// Information about the capacity and contents of a container for display in the UI
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class ContainerInfo
    {
        /// <summary>
        /// The container name to show to the player
        /// </summary>
        public readonly string DisplayName;

        /// <summary>
        /// The currently used volume of the container
        /// </summary>
        public readonly FixedPoint2 CurrentVolume;

        /// <summary>
        /// The maximum volume of the container
        /// </summary>
        public readonly FixedPoint2 MaxVolume;

        /// <summary>
        /// A list of the entities and their sizes within the container
        /// </summary>
        public List<(string Id, FixedPoint2 Quantity)>? Entities { get; init; }

        public List<ReagentQuantity>? Reagents { get; init; }

        public ContainerInfo(string displayName, FixedPoint2 currentVolume, FixedPoint2 maxVolume)
        {
            DisplayName = displayName;
            CurrentVolume = currentVolume;
            MaxVolume = maxVolume;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChemMasterBoundUserInterfaceState(
        ContainerInfo? containerInfo,
        IReadOnlyList<ReagentQuantity> bufferReagents,
        IReadOnlyList<ReagentQuantity> pillBufferReagents,
        FixedPoint2 bufferCurrentVolume,
        FixedPoint2 pillBufferCurrentVolume,
        uint selectedPillType,
        uint pillDosageLimit,
        bool updateLabel,
        int sortMethod,
        int transferringAmount)
        : BoundUserInterfaceState
    {
        public readonly ContainerInfo? ContainerInfo = containerInfo;

        /// <summary>
        /// A list of the reagents and their amounts within the buffer, if applicable.
        /// </summary>
        public readonly IReadOnlyList<ReagentQuantity> BufferReagents = bufferReagents;

        /// <summary>
        /// A list of the reagents and their amounts within the pill buffer, if applicable.
        /// </summary>
        public readonly IReadOnlyList<ReagentQuantity> PillBufferReagents = pillBufferReagents;

        public readonly FixedPoint2? BufferCurrentVolume = bufferCurrentVolume;
        public readonly FixedPoint2? PillBufferCurrentVolume = pillBufferCurrentVolume;

        public readonly uint SelectedPillType = selectedPillType;
        public readonly uint PillDosageLimit = pillDosageLimit;

        public readonly bool UpdateLabel = updateLabel;

        public readonly int SortMethod = sortMethod;
        public readonly int TransferringAmount = transferringAmount;
    }

    [Serializable, NetSerializable]
    public enum ChemMasterUiKey
    {
        Key
    }
}
