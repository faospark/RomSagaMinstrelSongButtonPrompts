param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$projectDir = $PSScriptRoot
$distDir = Join-Path $projectDir "dist"
$packageDir = Join-Path $distDir "package"
$zipName = "RomSagaMinstrelSongButtonPrompts-v$Version.zip"
$zipPath = Join-Path $distDir $zipName

Write-Host "Building RomSagaMinstrelSongButtonPrompts v$Version in Release configuration..." -ForegroundColor Cyan
dotnet build (Join-Path $projectDir "RomSagaMinstrelSongButtonPrompts.csproj") -c Release

if (Test-Path $distDir) {
    Remove-Item -Recurse -Force $distDir
}

$pluginTarget = Join-Path $packageDir "BepInEx\plugins\RomSagaMinstrelSongButtonPrompts"
$configTarget = Join-Path $packageDir "BepInEx\config"

New-Item -ItemType Directory -Force -Path $pluginTarget | Out-Null
New-Item -ItemType Directory -Force -Path $configTarget | Out-Null

Copy-Item (Join-Path $projectDir "bin\Release\net6.0\RomSagaMinstrelSongButtonPrompts.dll") $pluginTarget -Force
Copy-Item (Join-Path $projectDir "bin\Release\net6.0\RomSagaMinstrelSongButtonPrompts.pdb") $pluginTarget -Force

$defaultCfg = @"
## Settings file was created by plugin Romancing SaGa Minstrel Song Button Prompts v$Version
## Plugin GUID: com.romsagaminstrelsong.buttonprompts

[General]

## Controller button prompt style to display:
## Options: PS4 (DualShock 4 / PSX Colored), PS5 (DualSense Monochrome), Switch (Nintendo B/A/Y/X), XboxOne (Original Xbox), Steam, Keyboard, Auto.
# Setting type: PromptPlatform
# Default value: PS4
PromptStyle = PS4

## If true, gamepad prompts will be forced even when moving the mouse or pressing keyboard keys.
## If false (recommended), keyboard prompts are shown while using keyboard/mouse, and forced prompts are shown when using a controller.
# Setting type: Boolean
# Default value: false
ForceWhenUsingKeyboard = false

## Nintendo button layout (A/B and X/Y position swap).
## Options: Auto (automatic based on detected/forced controller), ForceTrue, ForceFalse.
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
"@

Set-Content -Path (Join-Path $configTarget "com.romsagaminstrelsong.buttonprompts.cfg") -Value $defaultCfg

Write-Host "Compressing release package to $zipPath..." -ForegroundColor Cyan
Compress-Archive -Path (Join-Path $packageDir "*") -DestinationPath $zipPath -Force

Write-Host "Release package successfully created at: $zipPath" -ForegroundColor Green

