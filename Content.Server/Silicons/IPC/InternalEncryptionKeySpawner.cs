using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Radio.Components; // Parkstation-IPC
using Content.Shared.Containers; // Parkstation-IPC
using Robust.Shared.Containers; // Parkstation-IPC

 // Pretty much copied from StationSpawningSystem.SpawnStartingGear
namespace Content.Server.Silicons.IPC;
public static class InternalEncryptionKeySpawner
{
    public static void TryInsertEncryptionKey(EntityUid target, StartingGearPrototype startingGear, IEntityManager entityManager, HumanoidCharacterProfile? profile)
    {
        if (entityManager.TryGetComponent<EncryptionKeyHolderComponent>(target, out var keyHolderComp))
        {
            var earEquipString = startingGear.GetGear("ears", profile);
            var containerMan = entityManager.System<SharedContainerSystem>();

            if (!string.IsNullOrEmpty(earEquipString))
            {
                var earEntity = entityManager.SpawnEntity(earEquipString, entityManager.GetComponent<TransformComponent>(target).Coordinates);

                if (entityManager.TryGetComponent<EncryptionKeyHolderComponent>(earEntity, out _) && // I had initially wanted this to spawn the headset, and simply move all the keys over, but the headset didn't seem to have any keys in it when spawned...
                    entityManager.TryGetComponent<ContainerFillComponent>(earEntity, out var fillComp) &&
                    fillComp.Containers.TryGetValue(EncryptionKeyHolderComponent.KeyContainerName, out var defaultKeys))
                {
                    containerMan.CleanContainer(keyHolderComp.KeyContainer);

                    foreach (var key in defaultKeys)
                    {
                        var keyEntity = entityManager.SpawnEntity(key, entityManager.GetComponent<TransformComponent>(target).Coordinates);
                        containerMan.Insert(keyEntity, keyHolderComp.KeyContainer, force: true);
                    }
                }

                entityManager.QueueDeleteEntity(earEntity);
            }
        }
    }
}
