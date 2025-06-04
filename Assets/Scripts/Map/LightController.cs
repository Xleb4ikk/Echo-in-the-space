using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField] private GameObject mapLight; // Ссылка на объект Map light
    [SerializeField] private AudioClip triggerSound; // Звук при активации триггера
    private Light[] childLights; // Массив для хранения всех дочерних источников света
    private AudioSource audioSource;
    private bool hasBeenTriggered = false; // Флаг для отслеживания активации

    private void Start()
    {
        // Получаем все компоненты Light у дочерних объектов
        if (mapLight != null)
        {
            childLights = mapLight.GetComponentsInChildren<Light>();
        }

        // Получаем или добавляем компонент AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, вошел ли игрок в триггер и не был ли он уже активирован
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            // Выключаем все дочерние источники света
            foreach (Light light in childLights)
            {
                light.enabled = false;
            }

            // Воспроизводим звук при активации триггера
            if (triggerSound != null)
            {
                audioSource.PlayOneShot(triggerSound);
            }

            // Отмечаем, что триггер был активирован
            hasBeenTriggered = true;
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
