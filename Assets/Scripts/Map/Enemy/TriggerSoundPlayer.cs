using UnityEngine;

public class TriggerSoundPlayer : MonoBehaviour
{
    public AudioClip soundToPlay;            // Звук, который будет воспроизводиться
    public Transform soundPlayPosition;      // Точка, где будет воспроизводиться звук
    public float volume = 1.0f;              // Громкость звука

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Воспроизводим звук в заданной точке
            if (soundToPlay != null && soundPlayPosition != null)
            {
                AudioSource.PlayClipAtPoint(soundToPlay, soundPlayPosition.position, volume);
            }
        }
    }
}
