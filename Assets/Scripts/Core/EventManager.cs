using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action<ButtonType, GameObject> OnModelClick;

    public static void ModelClicked(ButtonType type, GameObject model)
    {
        OnModelClick?.Invoke(type, model);
        Console.WriteLine("TT");
    }
}
