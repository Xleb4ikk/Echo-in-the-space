using System;
using UnityEngine;

[System.Serializable]
public class DisplaySettingsG
{
    public int resolutionWidth;
    public int resolutionHeight;
    public FullScreenMode fullscreen;  // <- обязательно FullScreenMode, а не bool
    public bool vSync;
}

