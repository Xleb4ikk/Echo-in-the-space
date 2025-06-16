using UnityEngine;

public class RotateTargetOnTrigger : MonoBehaviour
{
    public Transform targetObject; // bakedexitdoor
    public Vector3 desiredWorldEulerRotation; // (30.541, -90, 0)

    private void OnTriggerEnter(Collider other)
    {
        if (targetObject == null || targetObject.parent == null)
        {
            Debug.LogWarning("Не назначен targetObject или у него нет родителя.");
            return;
        }

        Quaternion worldRotation = Quaternion.Euler(desiredWorldEulerRotation);
        Quaternion parentRotation = targetObject.parent.rotation;

        // Вычисляем локальный поворот, чтобы глобальный стал нужным
        Quaternion localRotation = Quaternion.Inverse(parentRotation) * worldRotation;
        targetObject.localRotation = localRotation;

        Debug.Log("Установлен локальный поворот: " + localRotation.eulerAngles);
    }
}
