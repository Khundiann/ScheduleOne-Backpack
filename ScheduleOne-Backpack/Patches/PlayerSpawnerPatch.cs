using HarmonyLib;

#if IL2CPP
using Il2CppFishNet.Component.Spawning;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Storage;
#elif MONO
using FishNet.Component.Spawning;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Storage;
#endif

namespace Backpack.Patches;

[HarmonyPatch(typeof(PlayerSpawner))]
public static class PlayerSpawnerPatch
{
    [HarmonyPatch("InitializeOnce")]
    [HarmonyPostfix]
    public static void InitializeOnce(PlayerSpawner __instance)
    {
        var playerPrefab = __instance._playerPrefab;
        if (!playerPrefab)
        {
            Logger.Error("Player prefab is null!");
            return;
        }

        var player = playerPrefab.GetComponent<Player>();
        if (player == null)
        {
            Logger.Error("Player prefab does not have a Player component!");
            return;
        }

        EnsurePlayerBackpackSetup(player, false);
    }

    public static void EnsurePlayerBackpackSetup(Player player, bool addLocalBackpackComponent)
    {
        if (player == null)
            return;

        var storage = FindBackpackStorage(player) ?? player.gameObject.AddComponent<StorageEntity>();
        storage.SlotCount = PlayerBackpack.MaxStorageSlots;
        storage.DisplayRowCount = 8;
        storage.StorageEntityName = PlayerBackpack.StorageName;
        storage.MaxAccessDistance = float.PositiveInfinity;

        if (!addLocalBackpackComponent)
            return;

        var localGameObject = player.LocalGameObject != null ? player.LocalGameObject : player.gameObject;
        if (localGameObject.GetComponent<PlayerBackpack>() == null)
            localGameObject.AddComponent<PlayerBackpack>();
    }

    private static StorageEntity FindBackpackStorage(Player player)
    {
        foreach (var storage in player.gameObject.GetComponents<StorageEntity>())
        {
            if (storage.StorageEntityName == PlayerBackpack.StorageName)
                return storage;
        }

        return null;
    }
}
