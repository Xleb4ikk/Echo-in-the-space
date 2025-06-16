using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Animator doorAnimator;
    public string animationBoolName = "IsOpen";
    public bool Isopen = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && doorAnimator != null)
        {
            doorAnimator.SetBool(animationBoolName, Isopen);
        }
    }
}
