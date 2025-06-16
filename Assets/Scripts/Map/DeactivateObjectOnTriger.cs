using UnityEngine;

public class DeactivateObjectOnTrigger : MonoBehaviour
{
    [Tooltip("Объект, который будет деактивирован")]
    public GameObject objectToDeactivate;

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, имеет ли объект тег "Player"
        if (other.CompareTag("Player") && objectToDeactivate != null)
        {
            objectToDeactivate.SetActive(false);
        }
    }
}
