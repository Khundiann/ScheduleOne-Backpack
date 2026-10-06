using HarmonyLib;

#if IL2CPP
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.Persistence.Datas;
using Il2CppScheduleOne.PlayerScripts;
#elif MONO
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
#endif

namespace Backpack.Patches;

[HarmonyPatch(typeof(Player))]
public static class PlayerPatch
{
    [HarmonyPatch("Awake")]
    [HarmonyPrefix]
    public static void Awake(Player __instance)
    {
        PlayerSpawnerPatch.EnsurePlayerBackpackSetup(__instance, false);

        if (__instance.LocalExtraFiles.Contains("Backpack"))
            return;

        Logger.Info("Registering backpack file for player.");
        __instance.LocalExtraFiles.Add("Backpack");
    }

    [HarmonyPatch("WriteData")]
    [HarmonyPostfix]
    public static void WriteData(Player __instance, string parentFolderPath)
    {
        var backpackStorage = __instance.GetBackpackStorage();
        var contents = new ItemSet(backpackStorage.ItemSlots).GetJSON();

#if IL2CPP
        __instance.Cast<ISaveable>().WriteSubfile(parentFolderPath, "Backpack", contents);
#elif MONO
        ISaveable instance = __instance;
        instance.WriteSubfile(parentFolderPath, "Backpack", contents);
#endif
    }

    [HarmonyPatch("OnStartClient")]
    [HarmonyPostfix]
    public static void OnStartClient(Player __instance)
    {
        if (__instance == null || !__instance.IsOwner)
            return;

        PlayerSpawnerPatch.EnsurePlayerBackpackSetup(__instance, true);
    }

    [HarmonyPatch("LoadInventory")]
    [HarmonyPrefix]
    public static void LoadInventory(Player __instance, ref string contentsString)
    {
        if (string.IsNullOrEmpty(contentsString))
            return;

        var separatorIndex = contentsString.IndexOf("|||", StringComparison.Ordinal);
        if (separatorIndex < 0)
            return;

        var backpackData = contentsString[(separatorIndex + 3)..];
        contentsString = contentsString[..separatorIndex];

        // Every receiving player must remove the transport suffix before the game's
        // inventory deserializer sees it. Only the owner restores backpack contents.
        if (!__instance.IsOwner)
            return;

        Logger.Info("Loading backpack data from network.");
        try
        {
            var backpackStorage = __instance.GetBackpackStorage();
            if (!ItemSet.TryDeserialize(backpackData, out var itemSet))
            {
                Logger.Error("Failed to deserialize backpack data.");
                return;
            }

            itemSet.LoadTo(backpackStorage.ItemSlots);
        }
        catch (Exception e)
        {
            Logger.Error($"Error loading backpack data: {e.Message}");
        }
    }

}
