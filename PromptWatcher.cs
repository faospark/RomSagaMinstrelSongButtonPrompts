using System;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace RomSagaMinstrelSongButtonPrompts;

public class PromptWatcher : MonoBehaviour
{
    public PromptWatcher(IntPtr ptr) : base(ptr) { }

    public PromptWatcher() : base(ClassInjector.DerivedConstructorPointer<PromptWatcher>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    private void Update()
    {
        if (!Plugin.EnableHotkeysConfig.Value) return;

        var hotkey = Plugin.HotkeyCycleStyleConfig.Value;
        if (hotkey != KeyCode.None && Input.GetKeyDown(hotkey))
        {
            Plugin.CycleNextPromptStyle();
        }
    }
}

