using UnityEngine;

public class TriggerNotifier : MonoBehaviour
{
    public HideOnLookedAt hideScript;  // ������ �� ������ �������, ������� ������ ��������

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hideScript.PlayerEnteredTrigger();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hideScript.PlayerExitedTrigger();
        }
    }
}
