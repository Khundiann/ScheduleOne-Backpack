#if IL2CPP
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Storage;
#elif MONO
using ScheduleOne.PlayerScripts;
using ScheduleOne.Storage;
#endif

namespace Backpack;

public static class PlayerExtensions
{
    public static StorageEntity GetBackpackStorage(this Player player)
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));

        foreach (var storage in player.gameObject.GetComponents<StorageEntity>())
        {
            if (storage.StorageEntityName == PlayerBackpack.StorageName)
                return storage;
        }

        throw new InvalidOperationException("Player does not have a BackpackStorage component.");
    }
}
