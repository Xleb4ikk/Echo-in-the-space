using UnityEngine;

public class LaunchTrigger : MonoBehaviour
{
    public EscapePod escapePod; // ������ �� �������

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            escapePod.Launch();
        }
    }
}
