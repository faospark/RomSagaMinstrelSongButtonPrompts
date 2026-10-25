using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace RomSagaMinstrelSongButtonPrompts;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public class Plugin : BasePlugin
{
    public const string PluginGuid = "com.romsagaminstrelsong.buttonprompts";
    public const string PluginName = "Romancing SaGa Minstrel Song Button Prompts";
    public const string PluginVersion = "1.0.0";

    public static Plugin Instance { get; private set; } = null!;
    public static new ManualLogSource Log { get; private set; } = null!;

    public static ConfigEntry<PromptPlatform> PromptStyleConfig { get; private set; } = null!;
    public static ConfigEntry<bool> ForceWhenUsingKeyboardConfig { get; private set; } = null!;
    public static ConfigEntry<NintendoLayoutMode> NintendoLayoutConfig { get; private set; } = null!;
    public static ConfigEntry<bool> EnableHotkeysConfig { get; private set; } = null!;
    public static ConfigEntry<KeyCode> HotkeyCycleStyleConfig { get; private set; } = null!;

    private Harmony? _harmony;

    public override void Load()
    {
        Instance = this;
        Log = base.Log;

        Log.LogInfo($"Initializing {PluginName} v{PluginVersion}...");

        PromptStyleConfig = Config.Bind(
            "General",
            "PromptStyle",
            PromptPlatform.PS4,
            "Controller button prompt style to display:\n" +
            "Options: PS4 (DualShock 4 / PSX Colored), PS5 (DualSense Monochrome), Switch (Nintendo B/A/Y/X), XboxOne (Original Xbox), Steam, Keyboard, Auto."
        );

        ForceWhenUsingKeyboardConfig = Config.Bind(
            "General",
            "ForceWhenUsingKeyboard",
            false,
            "If true, gamepad prompts will be forced even when moving the mouse or pressing keyboard keys.\n" +
            "If false (recommended), keyboard prompts are shown while using keyboard/mouse, and forced prompts are shown when using a controller."
        );

        NintendoLayoutConfig = Config.Bind(
            "General",
            "NintendoLayout",
            NintendoLayoutMode.Auto,
            "Nintendo button layout (A/B and X/Y position swap).\n" +
            "Options: Auto (automatic based on detected/forced controller), ForceTrue, ForceFalse."
        );

        EnableHotkeysConfig = Config.Bind(
            "Hotkeys",
            "EnableHotkeys",
            true,
            "Enable in-game hotkeys to control button prompts."
        );

        HotkeyCycleStyleConfig = Config.Bind(
            "Hotkeys",
            "HotkeyCycleStyle",
            KeyCode.F9,
            "Hotkey to cycle to the next controller button prompt style (PS4 -> PS5 -> Switch -> XboxOne -> Auto)."
        );

        // Hook config change event to auto-refresh UI
        PromptStyleConfig.SettingChanged += (_, _) =>
        {
            Log.LogInfo($"Prompt style updated in config to: {PromptStyleConfig.Value}");
            TriggerRefresh();
        };

        // Register PromptWatcher MonoBehaviour for hotkey handling
        try
        {
            ClassInjector.RegisterTypeInIl2Cpp<PromptWatcher>();
            AddComponent<PromptWatcher>();
            Log.LogInfo("Registered PromptWatcher component successfully.");
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Failed to register PromptWatcher component: {ex}");
        }

        // Apply Harmony Patches
        try
        {
            _harmony = Harmony.CreateAndPatchAll(typeof(Patches));
            Log.LogInfo("Harmony patches applied successfully.");
        }
        catch (Exception ex)
        {
            Log.LogError($"Error applying Harmony patches: {ex}");
        }

        Log.LogInfo($"{PluginName} loaded! Active prompt style: {PromptStyleConfig.Value}");
    }

    public override bool Unload()
    {
        _harmony?.UnpatchSelf();
        return base.Unload();
    }

    public static void TriggerRefresh()
    {
        try
        {
            var iconCtrl = SingltonMonoBehaviourAsScene<ButtonIconController>.instance ?? UnityEngine.Object.FindObjectOfType<ButtonIconController>();
            if (iconCtrl != null)
            {
                iconCtrl.AdjustButtonIcon();
                Log.LogDebug("AdjustButtonIcon dispatched to refresh on-screen prompt glyphs.");
            }
        }
        catch (Exception ex)
        {
            Log.LogDebug($"Could not refresh icons immediately (menu may not be active): {ex.Message}");
        }
    }

    public static void CycleNextPromptStyle()
    {
        var current = PromptStyleConfig.Value;
        PromptPlatform next = current switch
        {
            PromptPlatform.PS4 => PromptPlatform.PS5,
            PromptPlatform.PS5 => PromptPlatform.Switch,
            PromptPlatform.Switch => PromptPlatform.XboxOne,
            PromptPlatform.XboxOne => PromptPlatform.Auto,
            PromptPlatform.Auto => PromptPlatform.PS4,
            _ => PromptPlatform.PS4
        };

        PromptStyleConfig.Value = next;
        Log.LogMessage($"[Button Prompts] Switched prompt style to: {next}");
        TriggerRefresh();
    }
}

