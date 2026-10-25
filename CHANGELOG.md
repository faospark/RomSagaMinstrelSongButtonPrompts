# Changelog

All notable changes to **RomSagaMinstrelSongButtonPrompts** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2026-10-25

### Added
- **Multi-Style Controller Prompts**:
  - `PS4`: DualShock 4 / PSX classic colored prompts (Cross, Circle, Square, Triangle, L1, R1, L2, R2, Options, Share).
  - `PS5`: DualSense minimal monochrome button glyphs (Cross, Circle, Square, Triangle, L1, R1, L2, R2, Create, Options).
  - `Switch`: Nintendo Switch button layout (B/A/Y/X, L, R, ZL, ZR, +, -).
  - `XboxOne`: Original Xbox One / Series button prompts.
  - `Steam`: Steam Controller prompt set.
  - `Keyboard`: Keyboard and Mouse glyphs.
  - `Auto`: Default game detection.
- **Dynamic TextMeshPro Tag Patching**: Intercepts in-game dialogue, tutorials, action prompts, and remapping menus to dynamically render the chosen controller glyphs.
- **Smart Input Detection (`ForceWhenUsingKeyboard`)**:
  - Automatically preserves keyboard and mouse prompts while using KBM, and switches seamlessly to PlayStation / custom prompts as soon as a gamepad is used.
- **In-Game Hotkey Cycle (`F9`)**:
  - Press **`F9`** anytime in gameplay to cycle through prompt styles (`PS4` &rarr; `PS5` &rarr; `Switch` &rarr; `XboxOne` &rarr; `Auto`) with instant on-screen UI refresh.
- **Live Configuration Hot-Reload**:
  - Automatically detects changes made to `BepInEx/config/com.romsagaminstrelsong.buttonprompts.cfg` and updates on-screen button icons immediately.
- **Nintendo Layout Support (`NintendoLayout`)**:
  - Option to toggle or force Nintendo button orientation (A/B and X/Y position swap).
- **Drag-and-Drop Release Bundle**:
  - Packaged with full `BepInEx/` folder hierarchy for instant drag-and-drop installation into the game root.

