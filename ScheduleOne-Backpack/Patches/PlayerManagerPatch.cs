using HarmonyLib;

using System.Reflection;

#if IL2CPP
using Il2CppScheduleOne.Persistence.Datas;
using Il2CppScheduleOne.Persistence.Loaders;
using Il2CppScheduleOne.PlayerScripts;
#elif MONO
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
#endif

namespace Backpack.Patches;

[HarmonyPatch(typeof(PlayerManager))]
public static class PlayerManagerPatch
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod() => AccessTools.Method(
        typeof(PlayerManager),
        "TryGetPlayerData",
        [typeof(string), typeof(bool), typeof(FullPlayerData).MakeByRefType()]);

    [HarmonyPostfix]
    public static void TryGetPlayerData(PlayerManager __instance, bool __result, FullPlayerData data)
    {
        if (!__result || data?.BasicData == null)
            return;

        var index = __instance.loadedPlayerData.IndexOf(data.BasicData);
        if (index < 0 || index >= __instance.loadedPlayerDataPaths.Count)
        {
            Logger.Warning("Failed to resolve loaded player data path for backpack sync.");
            return;
        }

        string dataPath = __instance.loadedPlayerDataPaths[index];
        if (string.IsNullOrWhiteSpace(dataPath))
            return;

        // A new player does not have Backpack.json until their first save.
        if (!File.Exists(Path.Combine(dataPath, "Backpack.json")))
            return;

        var loader = new PlayerLoader();
        if (!loader.TryLoadFile(dataPath, "Backpack", out var backpackString))
        {
            Logger.Warning("Failed to load player backpack under " + dataPath);
            return;
        }

        string inventoryString = data.InventoryString;
        inventoryString ??= string.Empty;
        string backpackContents = backpackString;
        var existingSuffix = inventoryString.IndexOf("|||", StringComparison.Ordinal);
        if (existingSuffix >= 0)
            inventoryString = inventoryString[..existingSuffix];

        data.InventoryString = inventoryString + "|||" + backpackContents;
    }
}
