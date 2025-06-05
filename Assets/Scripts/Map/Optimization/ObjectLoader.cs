using UnityEngine;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Map.Optimization
{
    public class ObjectLoader : MonoBehaviour
    {
        public static ObjectLoader Instance { get; private set; }

        [Header("Settings")]
        public ObjectLoaderSettings settings;

        // События
        public event Action<GameObject> OnObjectRegistered;
        public event Action<GameObject> OnObjectUnregistered;
        public event Action<GameObject, bool> OnObjectVisibilityChanged;

        private List<GameObject> loadableObjects = new List<GameObject>();
        private Queue<GameObject> registrationQueue = new Queue<GameObject>();
        private BoundingSphere[] boundingSpheres;
        private bool[] objectStates;
        private int sphereCapacity = 128;
        private int activeCount = 0;
        private CullingGroup[] cullingGroups;
        private Camera[] usedCameras;

        // Профилирование
        public int TotalObjects => loadableObjects.Count;
        public int ActiveObjects => activeCount;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (settings == null)
            {
                Debug.LogError("ObjectLoaderSettings не назначен!");
                enabled = false;
                return;
            }
            SetupCameras();
            InitCullingGroups();
        }

        private void SetupCameras()
        {
            if (settings.cameras != null && settings.cameras.Length > 0)
                usedCameras = settings.cameras;
            else
                usedCameras = new Camera[] { Camera.main };
        }

        private void InitCullingGroups()
        {
            cullingGroups = new CullingGroup[usedCameras.Length];
            for (int i = 0; i < usedCameras.Length; i++)
            {
                cullingGroups[i] = new CullingGroup();
                cullingGroups[i].targetCamera = usedCameras[i];
                cullingGroups[i].SetBoundingDistances(new float[] { settings.cullingDistance });
                cullingGroups[i].onStateChanged = OnCullingStateChanged;
            }
            boundingSpheres = new BoundingSphere[sphereCapacity];
            objectStates = new bool[sphereCapacity];
        }

        private void Update()
        {
            // Асинхронная регистрация
            int batch = settings.registrationBatchSize;
            while (registrationQueue.Count > 0 && batch-- > 0)
            {
                var obj = registrationQueue.Dequeue();
                InternalRegister(obj);
            }
        }

        public void RegisterObject(GameObject obj)
        {
            if (!loadableObjects.Contains(obj) && !registrationQueue.Contains(obj))
                registrationQueue.Enqueue(obj);
        }

        private void InternalRegister(GameObject obj)
        {
            if (loadableObjects.Count >= sphereCapacity)
            {
                // Увеличиваем массивы с запасом
                sphereCapacity = Mathf.CeilToInt(sphereCapacity * 1.5f);
                Array.Resize(ref boundingSpheres, sphereCapacity);
                Array.Resize(ref objectStates, sphereCapacity);
            }
            loadableObjects.Add(obj);
            UpdateBoundingSpheres();
            UpdateCullingGroups();
            OnObjectRegistered?.Invoke(obj);
        }

        public void UnregisterObject(GameObject obj)
        {
            int idx = loadableObjects.IndexOf(obj);
            if (idx >= 0)
            {
                loadableObjects.RemoveAt(idx);
                UpdateBoundingSpheres();
                UpdateCullingGroups();
                OnObjectUnregistered?.Invoke(obj);
            }
        }

        private void UpdateBoundingSpheres()
        {
            for (int i = 0; i < loadableObjects.Count; i++)
            {
                var go = loadableObjects[i];
                if (go != null)
                {
                    Bounds bounds = GetObjectBounds(go);
                    boundingSpheres[i] = new BoundingSphere(bounds.center, bounds.extents.magnitude);
                }
            }
        }

        private void UpdateCullingGroups()
        {
            for (int i = 0; i < cullingGroups.Length; i++)
            {
                cullingGroups[i].SetBoundingSpheres(boundingSpheres);
                cullingGroups[i].SetBoundingSphereCount(loadableObjects.Count);
            }
        }

        private Bounds GetObjectBounds(GameObject obj)
        {
            Collider collider = obj.GetComponent<Collider>();
            if (collider != null)
                return collider.bounds;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                return renderer.bounds;
            return new Bounds(obj.transform.position, Vector3.one);
        }

        private void OnCullingStateChanged(CullingGroupEvent evt)
        {
            if (evt.index >= 0 && evt.index < loadableObjects.Count)
            {
                GameObject obj = loadableObjects[evt.index];
                if (obj != null)
                {
                    bool isVisible = evt.isVisible && evt.currentDistance < settings.cullingDistance;
                    if (objectStates[evt.index] != isVisible)
                    {
                        objectStates[evt.index] = isVisible;
                        SetObjectActive(obj, isVisible);
                        OnObjectVisibilityChanged?.Invoke(obj, isVisible);
                        activeCount += isVisible ? 1 : -1;
                    }
                }
            }
        }

        private void SetObjectActive(GameObject obj, bool active)
        {
            if (settings.disableOnlyComponents)
            {
                var renderers = obj.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers) r.enabled = active;
                var colliders = obj.GetComponentsInChildren<Collider>(true);
                foreach (var c in colliders) c.enabled = active;
            }
            else
            {
                obj.SetActive(active);
            }
        }

        private void OnDestroy()
        {
            if (cullingGroups != null)
            {
                foreach (var cg in cullingGroups)
                    if (cg != null) cg.Dispose();
            }
            if (Instance == this) Instance = null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (boundingSpheres == null) return;
            Gizmos.color = Color.cyan;
            for (int i = 0; i < loadableObjects.Count; i++)
            {
                Gizmos.DrawWireSphere(boundingSpheres[i].position, boundingSpheres[i].radius);
            }
        }
#endif
    }
} 