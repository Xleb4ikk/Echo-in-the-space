using UnityEngine;

public class LadderFootstep : MonoBehaviour
{
    public AudioSource footstepAudioSource; // основной аудиосорс для шагов
    public AudioClip stairsClip;            // звук шагов по лестнице
    public AudioClip defaultClip;           // обычный звук шагов

    private bool onStairs = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Stairs"))
        {
            footstepAudioSource.clip = stairsClip;
            onStairs = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Stairs") && onStairs)
        {
            footstepAudioSource.clip = defaultClip;
            onStairs = false;
        }
    }
}
