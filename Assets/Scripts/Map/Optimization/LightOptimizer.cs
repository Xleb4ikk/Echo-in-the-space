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
    public float minVisibilityDistance = 5f; // Минимальная дистанция, на которой свет начинает затухать

    [Header("Optimization Settings")]
    [SerializeField] private int raysPerLight = 8; // Количество лучей для проверки каждого источника света
    [SerializeField] private float lightVisibilityThreshold = 0.3f; // Порог видимости (0-1)
    [SerializeField] private bool debugRays = false; // Отображать лучи в редакторе
    [SerializeField] private bool ignoreTransparent = true; // Игнорировать прозрачные материалы
    [SerializeField] private bool debugMode = true; // Добавляем режим отладки

    [Header("Player Settings")]
    [SerializeField] private Transform playerTransform; // Ссылка на трансформ игрока
    [SerializeField] private bool autoFollowPlayer = true; // Автоматически следовать за игроком

    private Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();
    private Dictionary<Light, float> targetIntensities = new Dictionary<Light, float>();
    private List<Light> allLights = new List<Light>();
    private float visibilityCheckTimer;
    private Transform playerCamera;

    private void Start()
    {
        // Находим камеру игрока
        playerCamera = Camera.main?.transform;
        if (playerCamera == null)
        {
            Debug.LogError("[LightOptimizer] Main camera not found!");
            return;
        }

        // Если не указан трансформ игрока и включено автоследование
        if (playerTransform == null && autoFollowPlayer)
        {
            // Пытаемся найти игрока по тегу
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                // Делаем LightOptimizer дочерним объектом игрока
                transform.SetParent(playerTransform);
                transform.localPosition = Vector3.zero;
                if (debugMode)
                {
                    Debug.Log("[LightOptimizer] Automatically attached to player");
                }
            }
            else
            {
                Debug.LogWarning("[LightOptimizer] Player not found! Please assign player manually or tag your player with 'Player' tag.");
            }
        }

        // Находим все источники света в сцене
        Light[] lights = FindObjectsOfType<Light>();
        if (lights.Length == 0)
        {
            Debug.LogWarning("[LightOptimizer] No lights found in the scene!");
        }

        foreach (Light light in lights)
        {
            if (light.type != LightType.Directional)
            {
                allLights.Add(light);
                originalIntensities[light] = light.intensity;
                targetIntensities[light] = 0f; // Начинаем с выключенного света
                
                if (debugMode)
                {
                    Debug.Log($"[LightOptimizer] Added light: {light.name}, Original intensity: {light.intensity}");
                }
            }
        }

        if (debugMode)
        {
            Debug.Log($"[LightOptimizer] Initialized with {allLights.Count} lights");
        }
    }

    private void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main?.transform;
            if (playerCamera == null) return;
        }

        visibilityCheckTimer += Time.deltaTime;
        if (visibilityCheckTimer >= visibilityCheckInterval)
        {
            visibilityCheckTimer = 0f;
            CheckLightsVisibility();
        }

        UpdateLightIntensities();
    }

    private bool IsValidObstacle(RaycastHit hit)
    {
        if (hit.collider == null) return false;

        // Проверяем, не является ли объект источником света
        if (hit.collider.GetComponent<Light>() != null)
            return false;

        // Проверяем наличие рендерера
        Renderer renderer = hit.collider.GetComponent<Renderer>();
        if (renderer == null) return false;

        if (ignoreTransparent)
        {
            // Проверяем материал на прозрачность
            Material material = renderer.material;
            if (material != null)
            {
                // Проверяем режим прозрачности материала
                if (material.GetTag("RenderType", false) == "Transparent")
                    return false;

                // Проверяем альфа-канал
                Color color = material.color;
                if (color.a < 0.9f)
                    return false;
            }
        }

        return true;
    }

    private void CheckLightsVisibility()
    {
        if (allLights.Count == 0)
        {
            if (debugMode) Debug.LogWarning("[LightOptimizer] No lights to check!");
            return;
        }

        Vector3 checkPosition = playerTransform != null ? playerTransform.position : playerCamera.position;

        foreach (Light light in allLights)
        {
            if (!light) continue;

            float distance = Vector3.Distance(checkPosition, light.transform.position);
            
            // Если свет находится дальше максимальной дистанции
            if (distance > maxVisibilityDistance)
            {
                targetIntensities[light] = 0f;
                continue;
            }

            // Базовая видимость в зависимости от расстояния
            float distanceRatio = Mathf.Clamp01((distance - minVisibilityDistance) / (maxVisibilityDistance - minVisibilityDistance));
            float baseVisibility = 1f - distanceRatio;

            // Проверяем видимость источника света с помощью лучей
            float visibleRays = 0;
            Vector3 lightPos = light.transform.position;
            float lightRadius = Mathf.Max(0.1f, light.range * 0.1f);

            for (int i = 0; i < raysPerLight; i++)
            {
                Vector3 randomOffset = Random.insideUnitSphere * lightRadius;
                Vector3 checkPoint = lightPos + randomOffset;
                Vector3 directionToLight = (checkPoint - checkPosition).normalized;

                bool rayHit = Physics.Raycast(checkPosition, directionToLight, out RaycastHit hit, distance);

                if (debugRays)
                {
                    Color rayColor = !rayHit ? Color.green : 
                                   (IsValidObstacle(hit) ? Color.red : Color.yellow);
                    Debug.DrawLine(checkPosition, checkPoint, rayColor, visibilityCheckInterval);
                }

                if (!rayHit || !IsValidObstacle(hit) || hit.distance >= distance - lightRadius)
                {
                    visibleRays++;
                }
            }

            float visibilityRatio = visibleRays / raysPerLight;
            float finalVisibility = baseVisibility * visibilityRatio;

            // Применяем порог видимости
            if (finalVisibility >= lightVisibilityThreshold)
            {
                targetIntensities[light] = originalIntensities[light] * finalVisibility;
            }
            else
            {
                targetIntensities[light] = 0f;
            }

            if (debugMode && Mathf.Abs(light.intensity - targetIntensities[light]) > 0.1f)
            {
                Debug.Log($"[LightOptimizer] Light {light.name}: Distance={distance:F1}, " +
                         $"Visible rays={visibleRays}/{raysPerLight}, " +
                         $"Base visibility={baseVisibility:F2}, " +
                         $"Final visibility={finalVisibility:F2}, " +
                         $"Target intensity={targetIntensities[light]:F2}");
            }
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

#if UNITY_EDITOR
    [CustomEditor(typeof(LightOptimizer))]
    public class LightOptimizerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Player Settings:\n" +
                "- Assign Player Transform или включите Auto Follow Player\n" +
                "- При Auto Follow Player скрипт автоматически прикрепится к игроку\n\n" +
                "Light Settings:\n" +
                "- Min Distance: Расстояние, на котором свет начинает затухать\n" +
                "- Max Distance: Расстояние, на котором свет полностью гаснет\n\n" +
                "Debug Settings:\n" +
                "- Debug Rays: Показывать лучи в редакторе\n" +
                "  - Зеленый = нет препятствий\n" +
                "  - Красный = есть препятствие\n" +
                "  - Желтый = прозрачное препятствие\n" +
                "- Debug Mode: Включить отладочные сообщения",
                MessageType.Info
            );

            LightOptimizer optimizer = (LightOptimizer)target;
            if (GUILayout.Button("Reinitialize Lights"))
            {
                optimizer.Start();
            }
        }
    }
#endif
} 