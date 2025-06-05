using UnityEngine;

namespace Map.Optimization
{
    [CreateAssetMenu(fileName = "ObjectLoaderSettings", menuName = "Optimization/ObjectLoaderSettings")]
    public class ObjectLoaderSettings : ScriptableObject
    {
        [Tooltip("Дистанция, на которой объекты будут активироваться")] public float cullingDistance = 50f;
        [Tooltip("Сколько объектов регистрировать за один кадр")] public int registrationBatchSize = 32;
        [Tooltip("Отключать только Renderer и Collider, а не весь объект")] public bool disableOnlyComponents = false;
        [Tooltip("Список камер для каллинга (если пусто — используется MainCamera)")] public Camera[] cameras;
    }
} 