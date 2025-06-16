using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Animator doorAnimator;
    public string animationBoolName = "IsOpen";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && doorAnimator != null)
        {
            doorAnimator.SetBool(animationBoolName, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && doorAnimator != null)
        {
            doorAnimator.SetBool(animationBoolName, false);
        }
    }
}
