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

                        Vector3 size = renderer.bounds.size;
                        Vector3 center = renderer.bounds.center - transform.position;

                        // Смещаем центр вниз на половину высоты
                        center.y -= size.y / 2f;

                        collider.size = size;
                        collider.center = center;
                    }
                    else
                    {
                        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
                        collider.size = Vector3.one;

                        // По умолчанию сместить вниз на 0.5
                        collider.center = new Vector3(0f, -0.5f, 0f);
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
