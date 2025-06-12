using System;
using UnityEngine;

[System.Serializable]
public class DisplaySettingsG
{
    public int resolutionWidth;
    public int resolutionHeight;
    public FullScreenMode fullscreen;  // <- ����������� FullScreenMode, � �� bool
    public bool vSync;
}

