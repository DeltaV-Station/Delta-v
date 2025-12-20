namespace Content.Shared._DV.Battery.Events;

/// <summary>
/// Raised when a piece of equipment with the BatteryProvider component
/// is equipped by a wearer.
/// Allows users to connect to this equipment by adding to the ConnectedEquipment set.
/// </summary>
/// <param name="Provider">Entity that is providing access to a battery.</param>
/// <param name="ConnectedEquipment">Modifiable set of equipment connected to this provider battery.</param>
[ByRefEvent]
public record struct BatteryProviderEquippedEvent(
    EntityUid Provider,
    HashSet<EntityUid> ConnectedEquipment
);

/// <summary>
/// Raised when a piece of equipment with the BatteryProvider component is
/// unequipped by a wearer.
/// </summary>
/// <param name="Provider">Entity that was providing access to a battery.</param>
[ByRefEvent]
public record struct BatteryProviderUnequippedEvent(
    EntityUid Provider
);
