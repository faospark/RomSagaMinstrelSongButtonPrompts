# RomSagaMinstrelSongButtonPrompts

A **BepInEx 6 (IL2CPP)** mod for **Romancing SaGa: Minstrel Song Remastered** (Steam) that replaces the default Xbox controller button prompt icons across the entire game (remapping screen, menus, battle HUD, action key guides, dialogue tags) with customized PlayStation (PS4 / PSX, PS5), Nintendo Switch, or custom button glyphs.

---

## Features

- **Multiple Controller Prompt Styles**:
  - `PS4`: Classic PlayStation / DualShock 4 styled button prompts with iconic colors ($\times$, $\bigcirc$, $\square$, $\triangle$, L1/R1, L2/R2, Share/Options).
  - `PS5`: DualSense styled button prompts (White / Minimalist glyphs).
  - `Switch`: Nintendo layout (B/A/Y/X layout, L, R, ZL, ZR, +, -). *(See Technical Note below)*
  - `XboxOne`: Xbox One / Series style button prompts.
  - `Steam`: Steam Controller prompt set.
  - `Keyboard`: Keyboard and mouse glyphs.
  - `Auto`: Game default automatic detection.

> [!NOTE]
> **Important Technical Note on Nintendo Switch Prompts**:
> Reverse engineering revealed that while Nintendo Switch architecture and enums (`ButtonIconPlatformType.Switch = 3`, `nxIconFilaPath = "NX/btn_list_nx"`) exist in the game engine's C# code and native jump tables, **no Nintendo Switch sprite sheet is bundled in the PC game assets** (the developers omitted `btn_list_nx` from PC `.assets`).
> On PC, selecting `Switch` utilizes the game's built-in neutral circular prompt set (`btn_list_steam`) combined with the game's native `IsNintendoLayout` button remapping logic (swapping Confirm to `B` and Cancel to `A`), giving an authentic Nintendo controller experience without missing button glyphs.

- **Dynamic Dialogue & Tag Replacement**: Patches TextMeshPro tags and `ButtonIconSpriteAssetContainer` to ensure in-game text prompts (`<sprite name="...">`) dynamically display the selected controller layout.
- **Smart Input Detection**: By default (`ForceWhenUsingKeyboard = false`), keyboard and mouse prompts are displayed when typing or moving the mouse, and seamlessly switch to your chosen PlayStation / custom controller prompts as soon as a gamepad is used.
- **In-Game Hotkey Cycle (`F9`)**: Press **`F9`** anytime during gameplay to cycle through styles on-the-fly (`PS4` &rarr; `PS5` &rarr; `Switch` &rarr; `XboxOne` &rarr; `Auto`) with instant on-screen UI refresh and automatic config persistence.
- **Live Reload Support**: Automatically reloads and updates on-screen prompt glyphs when configuration changes without requiring a game restart.

---

## Requirements

1. **Romancing SaGa -Minstrel Song- Remastered** (Steam edition, 64-bit Windows)
2. **BepInEx 6.0 (Unity IL2CPP x64)**
   - Download **[BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.697.zip](https://github.com/BepInEx/BepInEx/releases)** (or BepInEx 6.0.0-pre.2+).

---

## Installation Guide (Steam)

### 1. Locate Game Root Directory
In your Steam client:
1. Open your **Library** and right-click **Romancing SaGa -Minstrel Song- Remastered**.
2. Select **Manage** &rarr; **Browse local files**.
3. A file explorer window will open at your game's root directory:
   ```text
   ...\steamapps\common\Romancing SaGa -Minstrel Song- Remastered\
   ```
   *(This folder contains `Romancing SaGa -Minstrel Song- Remastered.exe`).*

---

### 2. Install BepInEx 6 (If not already installed)
1. Download **BepInEx 6.0 (Unity IL2CPP x64)**.
2. Extract the contents of the ZIP archive directly into your **`Romancing SaGa -Minstrel Song- Remastered`** root folder.
3. You should see `doorstop_config.ini`, `winhttp.dll`, and the `BepInEx/` directory alongside `Romancing SaGa -Minstrel Song- Remastered.exe`.
4. **Launch the game once via Steam**, then quit to desktop.
   - This generates necessary IL2CPP interop assemblies and configuration directories.

---

### 3. Install RomSagaMinstrelSongButtonPrompts
The release package is bundled with the complete directory structure for instant drag-and-drop installation.

1. Download **`RomSagaMinstrelSongButtonPrompts-vX.X.X.zip`** from [Releases](https://github.com/faospark/RomSagaMinstrelSongButtonPrompts/releases).
2. Open the ZIP archive. You will see a `BepInEx` folder.
3. **Drag and drop the `BepInEx` folder directly into your game's root directory**:
   ```text
   Romancing SaGa -Minstrel Song- Remastered/
   ├── BepInEx/
   │   ├── config/
   │   │   └── com.romsagaminstrelsong.buttonprompts.cfg
   │   └── plugins/
   │       └── RomSagaMinstrelSongButtonPrompts/
   │           └── RomSagaMinstrelSongButtonPrompts.dll
   ├── doorstop_config.ini
   └── Romancing SaGa -Minstrel Song- Remastered.exe
   ```
4. If prompted by Windows, choose **Replace the files in the destination**.

---

### 4. Verify Installation
1. Start the game through Steam.
2. In the BepInEx console or log, you will see:
   ```text
   [Info   :   BepInEx] Loading [Romancing SaGa Minstrel Song Button Prompts 1.0.1]
   [Info   :Romancing SaGa Minstrel Song Button Prompts] Initializing Romancing SaGa Minstrel Song Button Prompts v1.0.1...
   [Info   :Romancing SaGa Minstrel Song Button Prompts] Romancing SaGa Minstrel Song Button Prompts loaded! Active prompt style: PS4
   ```
3. Your controller prompts in the title screen, battle menus, and button remapping screen will now display PlayStation button glyphs.

---

## Configuration & Style Selection

Edit `BepInEx/config/com.romsagaminstrelsong.buttonprompts.cfg` with Notepad or any text editor:

```ini
[General]

## Controller button prompt style to display:
## Options: PS4, PS5, Switch, XboxOne, Steam, Keyboard, Auto
# Setting type: PromptPlatform
# Default value: PS4
PromptStyle = PS4

## If true, gamepad prompts will be forced even when moving the mouse or pressing keyboard keys.
## If false (recommended), keyboard prompts are shown while using keyboard/mouse, and forced prompts are shown when using a controller.
# Setting type: Boolean
# Default value: false
ForceWhenUsingKeyboard = false

## Nintendo button layout (A/B and X/Y position swap).
## Options: Auto, ForceTrue, ForceFalse.
# Setting type: NintendoLayoutMode
# Default value: Auto
NintendoLayout = Auto

[Hotkeys]

## Enable in-game hotkeys to control button prompts.
# Setting type: Boolean
# Default value: true
EnableHotkeys = true

## Hotkey to cycle to the next controller button prompt style (Default: F9).
# Setting type: KeyCode
# Default value: F9
HotkeyCycleStyle = F9
```

### In-Game Hotkey (`F9`)
You can cycle prompt styles on-the-fly without restarting the game or editing files:
- Press **`F9`** anytime during gameplay to cycle to the next style (`PS4` &rarr; `PS5` &rarr; `Switch` &rarr; `XboxOne` &rarr; `Auto`).
- The mod immediately updates both UI button sprites and dialogue text tags, and automatically persists your selection to `com.romsagaminstrelsong.buttonprompts.cfg`.
- The hotkey can be rebound to any Unity `KeyCode` or disabled via `EnableHotkeys = false`.

### Config File Hot-Reloading
You can also change settings externally while the game is running:
1. Edit `BepInEx/config/com.romsagaminstrelsong.buttonprompts.cfg`.
2. Change `PromptStyle = PS5` (or `PS4`, `Switch`, `XboxOne`, `Auto`) and save.
3. The mod will instantly hot-reload and update on-screen button icons immediately.

---

## Building from Source

Prerequisites:
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or newer
- BepInEx 6.x IL2CPP installed in your game directory

To compile:
```bash
dotnet build -c Release
```

To build a complete distributable release ZIP package:
```powershell
.\build_release.ps1 -Version 1.0.1
```

---

## License

This project is licensed under the [MIT License](LICENSE).
