using UnityEngine;

public class TriggerHandler : MonoBehaviour
{
    public TypewriterEffect typewriterEffect;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger");
            if (typewriterEffect != null)
            {
                typewriterEffect.ActivateDialogue();
            }
            else
            {
                Debug.LogError("TypewriterEffect reference is missing on " + gameObject.name);
            }
        }
    }
}