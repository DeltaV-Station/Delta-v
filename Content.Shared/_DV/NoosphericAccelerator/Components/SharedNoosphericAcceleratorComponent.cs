using Robust.Shared.Serialization;

namespace Content.Shared._DV.NoospericAccelerator.Components
{
    [NetSerializable, Serializable]
    public enum NoosphericAcceleratorVisuals
    {
        VisualState
    }

    [NetSerializable, Serializable]
    public enum NoosphericAcceleratorVisualState
    {
        //Open, //no prefix
        //Wired, //w prefix
        Unpowered, //c prefix
        Powered, //p prefix
        Level0, //0 prefix
        Level1, //1 prefix
        Level2, //2 prefix
        Level3 //3 prefix
    }

    [NetSerializable, Serializable]
    public enum NoosphericAcceleratorPowerState : byte
    {
        Standby = NoosphericAcceleratorVisualState.Powered,
        Level0 = NoosphericAcceleratorVisualState.Level0,
        Level1 = NoosphericAcceleratorVisualState.Level1,
        Level2 = NoosphericAcceleratorVisualState.Level2,
        Level3 = NoosphericAcceleratorVisualState.Level3,
    }

    public enum NoosphericAcceleratorVisualLayers
    {
        Base,
        Unlit
    }

    [Serializable, NetSerializable]
    public enum NoosphericAcceleratorWireStatus
    {
        Power,
        Keyboard,
        Limiter,
        Strength,
    }

    [NetSerializable, Serializable]
    public sealed class NoosphericAcceleratorUIState : BoundUserInterfaceState
    {
        public bool Assembled;
        public bool Enabled;
        public NoosphericAcceleratorPowerState State;
        public int PowerDraw;
        public int PowerReceive;

        //dont need a bool for the controlbox because... this is sent to the controlbox :D
        public bool EmitterStarboardExists;
        public bool EmitterForeExists;
        public bool EmitterPortExists;
        public bool PowerBoxExists;
        public bool FuelChamberExists;
        public bool EndCapExists;

        public bool InterfaceBlock;
        public NoosphericAcceleratorPowerState MaxLevel;
        public bool WirePowerBlock;

        public NoosphericAcceleratorUIState(bool assembled, bool enabled, NoosphericAcceleratorPowerState state, int powerReceive, int powerDraw, bool emitterStarboardExists, bool emitterForeExists, bool emitterPortExists, bool powerBoxExists, bool fuelChamberExists, bool endCapExists, bool interfaceBlock, NoosphericAcceleratorPowerState maxLevel, bool wirePowerBlock)
        {
            Assembled = assembled;
            Enabled = enabled;
            State = state;
            PowerDraw = powerDraw;
            PowerReceive = powerReceive;
            EmitterStarboardExists = emitterStarboardExists;
            EmitterForeExists = emitterForeExists;
            EmitterPortExists = emitterPortExists;
            PowerBoxExists = powerBoxExists;
            FuelChamberExists = fuelChamberExists;
            EndCapExists = endCapExists;
            InterfaceBlock = interfaceBlock;
            MaxLevel = maxLevel;
            WirePowerBlock = wirePowerBlock;
        }
    }

    [NetSerializable, Serializable]
    public sealed class NoosphericAcceleratorSetEnableMessage : BoundUserInterfaceMessage
    {
        public readonly bool Enabled;
        public NoosphericAcceleratorSetEnableMessage(bool enabled)
        {
            Enabled = enabled;
        }
    }

    [NetSerializable, Serializable]
    public sealed class NoosphericAcceleratorRescanPartsMessage : BoundUserInterfaceMessage
    {
        public NoosphericAcceleratorRescanPartsMessage()
        {
        }
    }

    [NetSerializable, Serializable]
    public sealed class NoosphericAcceleratorSetPowerStateMessage : BoundUserInterfaceMessage
    {
        public readonly NoosphericAcceleratorPowerState State;

        public NoosphericAcceleratorSetPowerStateMessage(NoosphericAcceleratorPowerState state)
        {
            State = state;
        }
    }

    [NetSerializable, Serializable]
    public enum NoosphericAcceleratorControlBoxUiKey
    {
        Key
    }
}
