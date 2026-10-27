using System;
using Constant.UI.ButtonIcon;
using HarmonyLib;

namespace RomSagaMinstrelSongButtonPrompts;

public static class Patches
{
    public static bool ShouldOverride(out ButtonIconPlatformType targetPlatform)
    {
        targetPlatform = ButtonIconPlatformType.None;

        var forced = Plugin.PromptStyleConfig.Value;
        if (forced == PromptPlatform.Auto)
            return false;

        // Check if player is using keyboard and user opted out of forcing over keyboard
        if (!Plugin.ForceWhenUsingKeyboardConfig.Value)
        {
            try
            {
                var input = CPlatformInput.Instance;
                if (input != null && input.InputType == eInputType.Keyboard)
                {
                    return false;
                }
            }
            catch
            {
                // Ignored if input singleton is not ready yet
            }
        }

        targetPlatform = forced switch
        {
            PromptPlatform.PS4 => ButtonIconPlatformType.PS4,
            PromptPlatform.PS5 => ButtonIconPlatformType.PS5,
            PromptPlatform.Switch => ButtonIconPlatformType.Switch,
            PromptPlatform.XboxOne => ButtonIconPlatformType.XboxOne,
            PromptPlatform.Steam => ButtonIconPlatformType.Steam,
            PromptPlatform.Keyboard => ButtonIconPlatformType.Keyboard,
            _ => ButtonIconPlatformType.None
        };

        return targetPlatform != ButtonIconPlatformType.None;
    }

    public static bool ShouldOverrideControllerType(out eControllerType targetController)
    {
        targetController = eControllerType.Unknown;

        var forced = Plugin.PromptStyleConfig.Value;
        if (forced == PromptPlatform.Auto)
            return false;

        if (!Plugin.ForceWhenUsingKeyboardConfig.Value)
        {
            try
            {
                var input = CPlatformInput.Instance;
                if (input != null && input.InputType == eInputType.Keyboard)
                {
                    return false;
                }
            }
            catch
            {
                // Ignored
            }
        }

        targetController = forced switch
        {
            PromptPlatform.PS4 => eControllerType.PS4Controller,
            PromptPlatform.PS5 => eControllerType.PS5Controller,
            PromptPlatform.Switch => eControllerType.SwitchProController,
            PromptPlatform.XboxOne => eControllerType.XBoxOneController,
            PromptPlatform.Steam => eControllerType.SteamController,
            _ => eControllerType.Unknown
        };

        return targetController != eControllerType.Unknown;
    }

    public static bool IsPlayStationTarget()
    {
        var forced = Plugin.PromptStyleConfig.Value;
        return forced == PromptPlatform.PS4 || forced == PromptPlatform.PS5;
    }

    // --- Harmony Patches ---

    /// <summary>
    /// Forces the central button icon platform query used by button containers and text tags.
    /// </summary>
    [HarmonyPatch(typeof(ButtonIconConstant), nameof(ButtonIconConstant.GetCurrentButtonIconPlatformType))]
    [HarmonyPostfix]
    public static void ButtonIconConstant_GetCurrentButtonIconPlatformType_Postfix(ref ButtonIconPlatformType __result)
    {
        if (ShouldOverride(out var target))
        {
            __result = target;
        }
    }

    /// <summary>
    /// Forces the platform type when retrieving specific sprite icons, protecting keyboard keys from corruption.
    /// On PC, Nintendo Switch circular prompts are located in the Steam sprite sheet (combined with NintendoLayout).
    /// </summary>
    [HarmonyPatch(typeof(ButtonIconContainer), nameof(ButtonIconContainer.GetButtonIconSprite))]
    [HarmonyPrefix]
    public static void ButtonIconContainer_GetButtonIconSprite_Prefix(ButtonType buttonType, ref ButtonIconPlatformType buttonPlatformType)
    {
        // Only override gamepad buttons (0 to 25). Values >= 26 are keyboard keys (Keyboard_Start_Num = 26).
        if ((int)buttonType < (int)ButtonType.Keyboard_Start_Num)
        {
            if (ShouldOverride(out var target))
            {
                // On PC, Switch prompts use the Steam circular button sprite sheet
                buttonPlatformType = target == ButtonIconPlatformType.Switch
                    ? ButtonIconPlatformType.Steam
                    : target;
            }
        }
    }

    /// <summary>
    /// Forces PlatformBridge controller type query.
    /// </summary>
    [HarmonyPatch(typeof(PlatformBridge), nameof(PlatformBridge.GetControllerType))]
    [HarmonyPostfix]
    public static void PlatformBridge_GetControllerType_Postfix(ref eControllerType __result)
    {
        if (ShouldOverrideControllerType(out var target))
        {
            __result = target;
        }
    }

    /// <summary>
    /// Forces CPlatformInput controller type query.
    /// </summary>
    [HarmonyPatch(typeof(CPlatformInput), nameof(CPlatformInput.GetControllerType))]
    [HarmonyPostfix]
    public static void CPlatformInput_GetControllerType_Postfix(ref eControllerType __result)
    {
        if (ShouldOverrideControllerType(out var target))
        {
            __result = target;
        }
    }

    /// <summary>
    /// Forces CPlatformInput.PadType getter.
    /// </summary>
    [HarmonyPatch(typeof(CPlatformInput), nameof(CPlatformInput.PadType), MethodType.Getter)]
    [HarmonyPostfix]
    public static void CPlatformInput_PadType_Postfix(ref eControllerType __result)
    {
        if (ShouldOverrideControllerType(out var target))
        {
            __result = target;
        }
    }

    /// <summary>
    /// Informs the game whether a PlayStation controller is active (enables PS4 key configurations).
    /// </summary>
    [HarmonyPatch(typeof(CPlatformInput), nameof(CPlatformInput.IsPS4Con), MethodType.Getter)]
    [HarmonyPostfix]
    public static void CPlatformInput_IsPS4Con_Postfix(ref bool __result)
    {
        if (Plugin.PromptStyleConfig.Value != PromptPlatform.Auto)
        {
            __result = IsPlayStationTarget();
        }
    }

    /// <summary>
    /// Allows overriding Nintendo layout (button A/B swap) if Switch prompts are active or user specifies.
    /// </summary>
    [HarmonyPatch(typeof(PlatformBridge), nameof(PlatformBridge.IsNintendoLayout))]
    [HarmonyPostfix]
    public static void PlatformBridge_IsNintendoLayout_Postfix(ref bool __result)
    {
        var mode = Plugin.NintendoLayoutConfig.Value;
        if (mode == NintendoLayoutMode.ForceTrue)
            __result = true;
        else if (mode == NintendoLayoutMode.ForceFalse)
            __result = false;
        else if (Plugin.PromptStyleConfig.Value == PromptPlatform.Switch)
            __result = true;
    }

    [HarmonyPatch(typeof(CPlatformInput), nameof(CPlatformInput.IsNintendoLayout), MethodType.Getter)]
    [HarmonyPostfix]
    public static void CPlatformInput_IsNintendoLayout_Postfix(ref bool __result)
    {
        var mode = Plugin.NintendoLayoutConfig.Value;
        if (mode == NintendoLayoutMode.ForceTrue)
            __result = true;
        else if (mode == NintendoLayoutMode.ForceFalse)
            __result = false;
        else if (Plugin.PromptStyleConfig.Value == PromptPlatform.Switch)
            __result = true;
    }
}
