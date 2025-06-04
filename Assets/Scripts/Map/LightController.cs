using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField] private GameObject mapLight; // Ссылка на объект Map light
    private Light[] childLights; // Массив для хранения всех дочерних источников света

    private void Start()
    {
        // Получаем все компоненты Light у дочерних объектов
        if (mapLight != null)
        {
            childLights = mapLight.GetComponentsInChildren<Light>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, вошел ли игрок в триггер
        if (other.CompareTag("Player"))
        {
            // Выключаем все дочерние источники света
            foreach (Light light in childLights)
            {
                light.enabled = false;
            }
        }
    }

    // private void OnTriggerExit(Collider other)
    // {
    //     // Проверяем, вышел ли игрок из триггера
    //     if (other.CompareTag("Player"))
    //     {
    //         // Включаем все дочерние источники света
    //         foreach (Light light in childLights)
    //         {
    //             light.enabled = true;
    //         }
    //     }
    // }
} 