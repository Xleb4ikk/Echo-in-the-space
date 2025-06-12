using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrahicsSettings : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

}
