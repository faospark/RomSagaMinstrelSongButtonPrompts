namespace RomSagaMinstrelSongButtonPrompts;

public enum PromptPlatform
{
    Auto = 0,
    PS4 = 1,      // PlayStation 4 / DualShock / PSX colored prompts (Cross, Circle, Square, Triangle, L1/R1/L2/R2, Share/Options)
    PS5 = 2,      // PlayStation 5 / DualSense monochrome prompts
    Switch = 3,   // Nintendo Switch prompts (A, B, X, Y, L, R, ZL, ZR, +, -)
    XboxOne = 4,  // Xbox One / Xbox Series / 360 prompts (A, B, X, Y, LB, RB, LT, RT)
    Steam = 5,    // Steam Controller prompts
    Keyboard = 6  // Keyboard / Mouse prompts
}

public enum NintendoLayoutMode
{
    Auto = 0,
    ForceTrue = 1,
    ForceFalse = 2
}

