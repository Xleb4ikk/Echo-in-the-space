using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LightZone : MonoBehaviour
{
    [Header("Zone Contents")]
    public List<Light> zoneLights = new List<Light>();
    public List<GameObject> zoneWalls = new List<GameObject>();

    [Header("Editor Settings")]
    [SerializeField] private bool visualizeZone = true;
    [SerializeField] private Vector3 zoneSize = new Vector3(10f, 3f, 10f); // Размер зоны для поиска

    private void Start()
    {
        // Удаляем коллайдер в игре, если он есть
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }

    // Проверить, находится ли точка внутри зоны
    public bool IsPointInZone(Vector3 point)
    {
        Bounds bounds = new Bounds(transform.position, zoneSize);
        return bounds.Contains(point);
    }

    // Получить все источники света в зоне
    public List<Light> GetZoneLights()
    {
        return zoneLights;
    }

    // Получить все стены в зоне
    public List<GameObject> GetZoneWalls()
    {
        return zoneWalls;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!visualizeZone) return;

        // Рисуем границы зоны
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.2f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, zoneSize);
        
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        Gizmos.DrawWireCube(Vector3.zero, zoneSize);

        // Рисуем связи со светом и стенами
        Gizmos.matrix = Matrix4x4.identity;
        
        // Рисуем линии к источникам света
        Gizmos.color = Color.yellow;
        foreach (Light light in zoneLights)
        {
            if (light != null)
                Gizmos.DrawLine(transform.position, light.transform.position);
        }

        // Рисуем линии к стенам
        Gizmos.color = Color.blue;
        foreach (GameObject wall in zoneWalls)
        {
            if (wall != null)
                Gizmos.DrawLine(transform.position, wall.transform.position);
        }
    }

    [CustomEditor(typeof(LightZone))]
    public class LightZoneEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LightZone zone = (LightZone)target;

            EditorGUILayout.Space();
            if (GUILayout.Button("Detect Zone Contents"))
            {
                DetectZoneContents(zone);
            }

            if (GUILayout.Button("Clear Zone Contents"))
            {
                zone.zoneLights.Clear();
                zone.zoneWalls.Clear();
                EditorUtility.SetDirty(zone);
            }
        }

        private void DetectZoneContents(LightZone zone)
        {
            zone.zoneLights.Clear();
            zone.zoneWalls.Clear();

            Bounds zoneBounds = new Bounds(zone.transform.position, zone.zoneSize);

            // Находим все источники света
            foreach (Light light in FindObjectsOfType<Light>())
            {
                if (light.type != LightType.Directional && zoneBounds.Contains(light.transform.position))
                {
                    zone.zoneLights.Add(light);
                }
            }

            // Находим все объекты-препятствия в зоне
            Collider[] colliders = Physics.OverlapBox(
                zoneBounds.center,
                zoneBounds.extents,
                zone.transform.rotation
            );

            foreach (Collider collider in colliders)
            {
                if (collider.isTrigger) continue;
                if (collider.GetComponent<Light>() != null) continue;

                if (collider.GetComponent<MeshRenderer>() != null || 
                    collider.GetComponent<MeshFilter>() != null)
                {
                    if (zoneBounds.Intersects(collider.bounds))
                    {
                        zone.zoneWalls.Add(collider.gameObject);
                    }
                }
            }

            Debug.Log($"Found {zone.zoneLights.Count} lights and {zone.zoneWalls.Count} walls in zone {zone.gameObject.name}");
            EditorUtility.SetDirty(zone);
        }
    }
#endif
} 