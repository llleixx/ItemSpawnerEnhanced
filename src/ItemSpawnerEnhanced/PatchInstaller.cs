using System;
using BepInEx.Logging;
using HarmonyLib;

namespace ItemSpawnerEnhanced;

internal static class PatchInstaller
{
    public static bool Install(Harmony harmony, ManualLogSource logger)
    {
        try
        {
            var original = AccessTools.Method(typeof(GUIManager), "Start");
            var postfix = AccessTools.Method(typeof(PatchCallbacks), nameof(PatchCallbacks.GuiManagerStartPostfix));
            if (original == null || postfix == null)
                return false;

            harmony.Patch(original, postfix: new HarmonyMethod(postfix));
            return true;
        }
        catch (Exception exception)
        {
            logger.LogError($"Failed to patch GUIManager.Start: {exception}");
            return false;
        }
    }
}

internal static class PatchCallbacks
{
    public static void GuiManagerStartPostfix(GUIManager __instance) => Plugin.Instance?.Attach(__instance);
}
