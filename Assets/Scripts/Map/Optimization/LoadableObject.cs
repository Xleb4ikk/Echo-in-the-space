using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Map.Optimization
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Optimization/LoadableObject")]
    public class LoadableObject : MonoBehaviour
    {
        private ObjectLoader objectLoader;
        private bool registered = false;

        private void Awake()
        {
            // Проверяем наличие коллайдера
            if (GetComponent<Collider>() == null)
            {
                Light light = GetComponent<Light>();
                if (light != null)
                {
                    SphereCollider collider = gameObject.AddComponent<SphereCollider>();
                    collider.radius = light.range;
                    collider.isTrigger = true;
                }
                else
                {
                    Renderer renderer = GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
                        collider.center = renderer.bounds.center - transform.position;
                        collider.size = renderer.bounds.size;
                    }
                    else
                    {
                        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
                        collider.size = Vector3.one;
                    }
                }
            }
        }

        private void Start()
        {
            objectLoader = ObjectLoader.Instance;
            if (objectLoader != null && !registered)
            {
                objectLoader.RegisterObject(gameObject);
                registered = true;
            }
        }

        private void OnDestroy()
        {
            if (objectLoader != null && registered)
            {
                objectLoader.UnregisterObject(gameObject);
                registered = false;
            }
        }

        // Для ObjectLoader: получить BoundingSphere этого объекта
        public BoundingSphere GetBoundingSphere()
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null)
                return new BoundingSphere(collider.bounds.center, collider.bounds.extents.magnitude);
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
                return new BoundingSphere(renderer.bounds.center, renderer.bounds.extents.magnitude);
            return new BoundingSphere(transform.position, 1f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            var sphere = GetBoundingSphere();
            Gizmos.DrawWireSphere(sphere.position, sphere.radius);
        }
#endif
    }
} 