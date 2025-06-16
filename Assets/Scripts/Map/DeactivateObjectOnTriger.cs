using UnityEngine;

public class DeactivateObjectOnTrigger : MonoBehaviour
{
    [Tooltip("������, ������� ����� �������������")]
    public GameObject objectToDeactivate;

    private void OnTriggerEnter(Collider other)
    {
        // ���������, ����� �� ������ ��� "Player"
        if (other.CompareTag("Player") && objectToDeactivate != null)
        {
            objectToDeactivate.SetActive(false);
        }
    }
}
