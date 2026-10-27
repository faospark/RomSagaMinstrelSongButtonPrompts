# Romancing SaGa: Minstrel Song Remastered — Controller Prompt Subsystem Reverse Engineering Report

## Executive Summary

During reverse engineering of *Romancing SaGa: Minstrel Song Remastered* (Steam / PC version), static binary analysis, PE/CLI metadata inspection, and native x86-64 disassembly revealed that:
1. **The PC build already bundles the complete set of PlayStation 4 (DualShock 4 / PSX style) and PlayStation 5 (DualSense) prompt textures and sprite assets**, alongside Nintendo Switch, Xbox, and Steam controller textures.
2. The game uses a centralized platform-evaluation routine in `Constant.UI.ButtonIcon.ButtonIconConstant` which maps detected gamepad hardware (`eControllerType`) to an icon set (`ButtonIconPlatformType`).
3. For PC, any unrecognized or XInput controller defaults to **Xbox One** prompts (`ButtonIconPlatformType.XboxOne = 4`).
4. By intercepting 7 key IL2CPP methods with Harmony, we can redirect all on-screen gamepad prompt rendering to **PlayStation** (or any other bundled prompt set) while safely preserving keyboard/mouse prompts when typing or moving the mouse.

---

## 1. Target Architecture & Environment

| Component | Target Specification |
| :--- | :--- |
| **Engine** | Unity 2020.3.36f1 (64-bit IL2CPP) |
| **Native Module** | `GameAssembly.dll` (49.4 MB) |
| **Loader** | BepInEx 6 IL2CPP (`6.0.0-be.697`) on .NET 6.0.7 runtime |
| **Metadata** | IL2CPP Metadata version 27.1 (`global-metadata.dat`) |
| **Extracted Interop** | `BepInEx/interop/Assembly-CSharp.dll` (21.1 MB, 3,586 TypeDefs, 120,401 MethodDefs) |
| **Mapping DBs** | `BepInEx/interop/MethodAddressToToken.db` & `MethodXrefScanCache.db` |

---

## 2. Discovered Enums & Data Structures

### A. `Constant.UI.ButtonIcon.ButtonIconPlatformType`
This enum governs which sub-dictionary of sprites is queried for button prompts:
```csharp
namespace Constant.UI.ButtonIcon
{
    public enum ButtonIconPlatformType
    {
        None     = 0,
        PS4      = 1, // DualShock 4 / PSX prompts (Cross, Circle, Square, Triangle, L1, R1, L2, R2, Share, Options)
        PS5      = 2, // DualSense prompts (Cross, Circle, Square, Triangle, L1, R1, L2, R2, Create, Options)
        Switch   = 3, // Nintendo Switch prompts (A, B, X, Y, +, -, L, R, ZL, ZR)
        XboxOne  = 4, // Xbox One / Series prompts (A, B, X, Y, LB, RB, LT, RT, View, Menu)
        Steam    = 5, // Steam Controller prompts
        Keyboard = 6  // PC Keyboard / Mouse prompts
    }
}
```

### B. `global::eControllerType`
Hardware classification enum reported by the input subsystem:
```csharp
public enum eControllerType
{
    Unknown             = 0,
    SteamController     = 1,
    XBoxController      = 2,
    XBox360Controller   = 3,
    XBoxOneController   = 4,
    PS4Controller       = 5,
    PS5Controller       = 6,
    AppleMFiController  = 7,
    AndroidController   = 8,
    SwitchJoyConPair    = 9,
    SwitchJoyConSingle  = 10,
    SwitchProController = 11
}
```

### C. `global::eInputType`
Distinguishes active device types:
```csharp
public enum eInputType
{
    None       = 0,
    Controller = 1,
    Keyboard   = 2
}
```

### D. `Constant.UI.ButtonIcon.ButtonType`
Encompasses both controller inputs and keyboard scancodes:
- Gamepad Buttons: `Pad_RD = 0` (Right-Down: Xbox A, PS Cross, Switch B) through `Pad_R_Stick_D = 25`.
- Keyboard Keys: `Keyboard_Start_Num = 26` (`Keyboard_F1 = 27` through `Keyboard_End_Num = 130`).

> [!NOTE]
> Separating gamepad buttons (`< 26`) from keyboard keys (`>= 26`) is crucial: overriding the platform query for keyboard scancodes would break keyboard icon rendering.

---

## 3. Bundled Asset Inventory

Static string analysis of `global-metadata.dat` revealed that all console textures are shipped inside the PC version's Unity asset bundles:

```
ui/Texture/Common/ButtonIcon/PS4/btn_list_ps4-     <- Full PS4/PSX prompt spritesheet
ui/Texture/Common/ButtonIcon/PS5/btn_list_ps51     <- Full PS5 DualSense prompt spritesheet
ui/Texture/Common/ButtonIcon/NX/btn_list_nx-       <- Nintendo Switch prompt spritesheet
ui/Texture/Common/ButtonIcon/XB1/btn_list_xb1      <- Xbox One / Series prompt spritesheet
ui/Texture/Common/ButtonIcon/Steam/btn_list_steam4 <- Steam controller prompt spritesheet
ui/Texture/Common/ButtonIcon/Steam/btn_list_keyboard- <- Keyboard / Mouse spritesheet
ui/Texture/Common/ButtonIcon/Common/btn_list_plus  <- Plus / combo button icon
```

TextMeshPro Sprite Asset definitions:
- `SpriteAssetNamePS4`
- `SpriteAssetNamePS5`
- `SpriteAssetNameNX`
- `SpriteAssetNameSteam`

---

## 4. Disassembled Native Routines (`GameAssembly.dll`)

### 4.1. Platform Decision Point: `ButtonIconConstant::GetCurrentButtonIconPlatformType`
- **RVA:** `0xBFD170` | **File Offset:** `0xBFB770`
- **Disassembly Analysis:**
```nasm
0000000000BFD1BF  mov rcx,[rax]             ; PlatformBridge.Instance
0000000000BFD1C2  test rcx,rcx
0000000000BFD1C5  je short 0000000000BFD22Bh
0000000000BFD1C7  xor edx,edx
0000000000BFD1C9  call 0000000000D66310h    ; PlatformBridge::GetControllerType()
0000000000BFD1CE  cmp eax,0Bh               ; Validate <= 11 (SwitchProController)
0000000000BFD1D1  ja short 0000000000BFD224h
0000000000BFD1D3  lea rdx,[rel 0]
0000000000BFD1DC  mov ecx,[rdx+rax*4+0BFD234h] ; Jump table indexed by eControllerType
0000000000BFD1E3  add rcx,rdx
0000000000BFD1E6  jmp rcx
; Jump table outcomes:
0000000000BFD1E8  mov eax,6                 ; Keyboard (6)
0000000000BFD1F2  mov eax,5                 ; Steam (5)
0000000000BFD1FC  mov eax,1                 ; PS4 (1)
0000000000BFD206  mov eax,2                 ; PS5 (2)
0000000000BFD210  mov eax,4                 ; XboxOne (4)
0000000000BFD21A  mov eax,3                 ; Switch (3)
0000000000BFD224  xor eax,eax               ; None (0)
```
*Takeaway:* This routine is the single source of truth for the active UI prompt platform. Overriding its return value cascades to all prompt queries.

---

### 4.2. Hardware Input Query: `PlatformBridge::GetControllerType` & `CPlatformInput::GetControllerType`
- **RVA:** `0xD66310` & `0xC061B0`
- **Disassembly Analysis:**
```nasm
0000000000D6635F  mov rcx,[rax]             ; CPlatformInput.Instance
0000000000D66367  cmp dword [rcx+64h],2     ; Check if eInputType == 2 (Keyboard)
0000000000D6636B  je short 0000000000D66375h
0000000000D6636D  mov eax,[rcx+58h]         ; Return CPlatformInput.PadType (eControllerType)
0000000000D66374  ret
0000000000D66375  xor eax,eax               ; Returns Unknown (0) if Keyboard is active
```

---

### 4.3. Sprite Retrieval: `ButtonIconContainer::GetButtonIconSprite`
- **RVA:** `0xBFF5A0`
- **Internal Storage:**
```csharp
Dictionary<ButtonIconPlatformType, Dictionary<ButtonType, Sprite>> buttonIconDict;
```
When `buttonIconDict` is empty on first call, it automatically triggers `LoadIcon()` (`0xBFEA80`), which populates every platform's spritesheet into memory.

---

### 4.4. UI Refresh Mechanism: `ButtonIconController::AdjustButtonIcon`
- **RVA:** `0xBFFD40`
- Iterates over all registered `KeyAssignButtonIcon` instances in active menus/HUDs and dispatches `AdjustButtonIcon()`.
- Calling `ButtonIconController.Instance.AdjustButtonIcon()` forces immediate visual update without reloading scenes.

---

## 5. Harmony Interception Architecture

```
                  ┌────────────────────────────────────────┐
                  │          BepInEx 6 IL2CPP Plugin       │
                  │              (PSXPrompts.dll)          │
                  └───────────────────┬────────────────────┘
                                      │
        ┌─────────────────────────────┼─────────────────────────────┐
        ▼                             ▼                             ▼
┌───────────────────────┐   ┌───────────────────────┐   ┌───────────────────────┐
│ ButtonIconConstant    │   │  ButtonIconContainer  │   │     CPlatformInput    │
│ .GetCurrentPlatform() │   │  .GetButtonIconSprite │   │  .GetControllerType() │
│ [Postfix Override]    │   │  [Prefix Override]    │   │  [Postfix Override]   │
└───────────────────────┘   └───────────────────────┘   └───────────────────────┘
```

### Applied Hooks:
1. `ButtonIconConstant.GetCurrentButtonIconPlatformType()` [Postfix]: Returns forced `ButtonIconPlatformType` (`PS4`, `PS5`, `Switch`, etc.).
2. `ButtonIconContainer.GetButtonIconSprite(ButtonType, ref ButtonIconPlatformType)` [Prefix]: Overrides platform argument for gamepad buttons (`< 26`).
3. `PlatformBridge.GetControllerType()` & `CPlatformInput.GetControllerType()` [Postfix]: Returns forced `eControllerType` (`PS4Controller`, etc.).
4. `CPlatformInput.PadType` getter [Postfix]: Returns forced `eControllerType`.
5. `CPlatformInput.IsPS4Con` getter [Postfix]: Returns `true` for PlayStation targets, activating native PS4 key mapping options.
6. `ButtonIconController.conectControllerType` getter [Postfix]: Keeps the controller state synchronized.
7. `PlatformBridge.IsNintendoLayout` & `CPlatformInput.IsNintendoLayout` [Postfix]: Manages A/B and X/Y button swapping for Nintendo layouts.

---

## 6. Deliverables & Artifacts

- **Plugin Binary:** `BepInEx/plugins/PSXPrompts.dll`
- **Source Code:** `plugins_src/PSXPrompts/`
  - `PSXPromptsPlugin.cs` (Lifecycle, configuration, hotkey coordination)
  - `Patches.cs` (7 Harmony patch routines)
  - `PromptWatcher.cs` (IL2CPP MonoBehaviour listening for `F9` toggle)
  - `PromptPlatform.cs` (Enums and mapping utilities)
  - `PSXPrompts.csproj` (Targeting `net6.0` with auto-copy to `plugins/`)
- **Documentation:** `plugins_src/PSXPrompts/README.md`
