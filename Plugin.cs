using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace QuickestRestart;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static ConfigEntry<KeyCode> RestartKey { get; set; }
    public static ConfigEntry<KeyCode> QuitKey { get; set; }
    private const KeyCode DEFAULT_RESTART_KEY = KeyCode.R;
    private const KeyCode DEFAULT_QUIT_KEY = KeyCode.Q;
    private static bool actuallyQuitting;

    public void Awake()
    {
        RestartKey = Config.Bind("KeyBinds", "RestartKey", DEFAULT_RESTART_KEY);
        QuitKey = Config.Bind("KeyBinds", "QuitKey", DEFAULT_QUIT_KEY);
        new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
    }

    [HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
    public static class GameControllerUpdatePatches
    {
        public static void Postfix(GameController __instance)
        {
            if (__instance.retrying || actuallyQuitting) return;
            if (Input.GetKey(RestartKey.Value))
            {
                __instance.quitting = true; // __instance.quitting means paused :skull:
                __instance.pauseRetryLevel();
            }
            else if (Input.GetKey(QuitKey.Value))
            {
                __instance.quitting = true;
                __instance.pauseQuitLevel();
            }
        }
    }

    [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
    public static class GameControllerStartPatches
    {
        public static void Postfix() => actuallyQuitting = false;
    }

    [HarmonyPatch(typeof(GameController), nameof(GameController.pauseQuitLevel))]
    public static class GameControllerPauseQuitLevelPatches
    {
        public static void Prefix() => actuallyQuitting = true;
    }
}
