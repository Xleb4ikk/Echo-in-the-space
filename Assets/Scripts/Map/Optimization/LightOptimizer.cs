using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LightOptimizer : MonoBehaviour
{
    [Header("Light Settings")]
    public float transitionSpeed = 2f; // Скорость перехода света
    public float visibilityCheckInterval = 0.2f; // Интервал проверки видимости
    public float maxVisibilityDistance = 20f; // Максимальная дистанция проверки видимости
    public LayerMask obstacleLayer; // Слой для препятствий

    [Header("Optimization Settings")]
    [SerializeField] private int raysPerLight = 8; // Количество лучей для проверки каждого источника света
    [SerializeField] private float lightVisibilityThreshold = 0.3f; // Порог видимости (0-1)
    [SerializeField] private bool debugRays = false; // Отображать лучи в редакторе

    [Header("Zone Settings")]
    [SerializeField] private bool autoFindZones = true;
    [SerializeField] private List<LightZone> lightZones = new List<LightZone>();

    private Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();
    private Dictionary<Light, float> targetIntensities = new Dictionary<Light, float>();
    private List<Light> allLights = new List<Light>();
    private float visibilityCheckTimer;
    private Transform playerCamera;
    private LightZone currentZone;

    private void Start()
    {
        if (autoFindZones)
        {
            FindAllZones();
        }

        // Находим все источники света в сцене
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type != LightType.Directional) // Пропускаем directional lights
            {
                allLights.Add(light);
                originalIntensities[light] = light.intensity;
                targetIntensities[light] = 0f;
                light.enabled = true;
                light.intensity = 0f;
            }
        }

        // Находим камеру игрока
        playerCamera = Camera.main.transform;
    }

    private void Update()
    {
        if (playerCamera == null) return;

        visibilityCheckTimer += Time.deltaTime;
        if (visibilityCheckTimer >= visibilityCheckInterval)
        {
            visibilityCheckTimer = 0f;
            UpdateCurrentZone();
            CheckLightsVisibility();
        }

        UpdateLightIntensities();
    }

    private void InitializeLights()
    {
        originalIntensities.Clear();
        targetIntensities.Clear();

        foreach (var zone in lightZones)
        {
            foreach (var light in zone.GetZoneLights())
            {
                if (light && !originalIntensities.ContainsKey(light))
                {
                    originalIntensities[light] = light.intensity;
                    targetIntensities[light] = 0f;
                    light.enabled = true;
                    light.intensity = 0f;
                }
            }
        }
    }

    private void UpdateCurrentZone()
    {
        Vector3 cameraPos = playerCamera.position;
        LightZone newZone = null;

        foreach (var zone in lightZones)
        {
            if (zone.IsPointInZone(cameraPos))
            {
                newZone = zone;
                break;
            }
        }

        if (newZone != currentZone)
        {
            currentZone = newZone;
            // При смене зоны можно добавить дополнительную логику
        }
    }

    private void CheckLightsVisibility()
    {
        Vector3 cameraPos = playerCamera.position;

        foreach (Light light in allLights)
        {
            if (!light) continue;

            float distance = Vector3.Distance(cameraPos, light.transform.position);
            if (distance > maxVisibilityDistance)
            {
                targetIntensities[light] = 0f;
                continue;
            }

            // Проверяем видимость источника света с помощью нескольких лучей
            float visibleRays = 0;
            Vector3 lightPos = light.transform.position;
            float lightRadius = Mathf.Max(0.1f, light.range * 0.1f); // Используем радиус света для проверки

            for (int i = 0; i < raysPerLight; i++)
            {
                // Генерируем случайную точку вокруг источника света
                Vector3 randomOffset = Random.insideUnitSphere * lightRadius;
                Vector3 checkPoint = lightPos + randomOffset;

                // Направление от камеры к точке проверки
                Vector3 directionToLight = (checkPoint - cameraPos).normalized;

                // Проверяем препятствия
                bool rayHit = Physics.Raycast(cameraPos, directionToLight, out RaycastHit hit, distance, obstacleLayer);

                if (debugRays)
                {
                    // Визуализация лучей в редакторе
                    Debug.DrawLine(cameraPos, checkPoint, 
                        rayHit ? Color.red : Color.green, 
                        visibilityCheckInterval);
                }

                // Если луч не попал в препятствие или попал в сам источник света
                if (!rayHit || hit.distance >= distance - lightRadius)
                {
                    visibleRays++;
                }
            }

            // Вычисляем процент видимости
            float visibilityRatio = visibleRays / raysPerLight;
            
            // Если видимость выше порога, считаем свет видимым
            targetIntensities[light] = visibilityRatio >= lightVisibilityThreshold ? 
                originalIntensities[light] * visibilityRatio : 0f;
        }
    }

    private void UpdateLightIntensities()
    {
        foreach (var light in allLights)
        {
            if (!light) continue;

            float targetIntensity = targetIntensities[light];
            if (!Mathf.Approximately(light.intensity, targetIntensity))
            {
                light.intensity = Mathf.MoveTowards(
                    light.intensity,
                    targetIntensity,
                    transitionSpeed * Time.deltaTime
                );
            }
        }
    }

    public void FindAllZones()
    {
        lightZones.Clear();
        LightZone[] zones = FindObjectsOfType<LightZone>();
        lightZones.AddRange(zones);
        Debug.Log($"Found {lightZones.Count} light zones");
    }

    // Добавить новую зону
    public void AddZone(LightZone zone)
    {
        if (zone && !lightZones.Contains(zone))
        {
            lightZones.Add(zone);
            // Инициализируем источники света из новой зоны
            foreach (var light in zone.GetZoneLights())
            {
                if (light && !originalIntensities.ContainsKey(light))
                {
                    originalIntensities[light] = light.intensity;
                    targetIntensities[light] = 0f;
                    light.enabled = true;
                    light.intensity = 0f;
                }
            }
        }
    }

    // Удалить зону
    public void RemoveZone(LightZone zone)
    {
        if (zone && lightZones.Contains(zone))
        {
            lightZones.Remove(zone);
            // Восстанавливаем оригинальные настройки света
            foreach (var light in zone.GetZoneLights())
            {
                if (light && originalIntensities.ContainsKey(light))
                {
                    light.intensity = originalIntensities[light];
                    originalIntensities.Remove(light);
                    targetIntensities.Remove(light);
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LightOptimizer))]
    public class LightOptimizerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LightOptimizer optimizer = (LightOptimizer)target;

            EditorGUILayout.Space();
            if (GUILayout.Button("Find All Zones"))
            {
                optimizer.FindAllZones();
            }

            if (GUILayout.Button("Reinitialize Lights"))
            {
                optimizer.InitializeLights();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Rays Per Light: Больше лучей = точнее проверка, но больше нагрузка\n" +
                "Visibility Threshold: Минимальный процент видимых лучей для активации света\n" +
                "Debug Rays: Показывать лучи в редакторе (зеленый = видимый, красный = блокирован)",
                MessageType.Info
            );
        }
    }
#endif
} 