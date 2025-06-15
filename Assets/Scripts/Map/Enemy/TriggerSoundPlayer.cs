using UnityEngine;

public class TriggerSoundPlayer : MonoBehaviour
{
    public AudioClip soundToPlay;            // Звук, который будет воспроизводиться
    public Transform soundPlayPosition;      // Точка, где будет воспроизводиться звук
    public float volume = 1.0f;              // Громкость звука

    private bool hasPlayed = false;          // Флаг, чтобы звук проигрывался один раз

    private void OnTriggerEnter(Collider other)
    {
        if (!hasPlayed && other.CompareTag("Player"))
        {
            if (soundToPlay != null && soundPlayPosition != null)
            {
                AudioSource.PlayClipAtPoint(soundToPlay, soundPlayPosition.position, volume);
                hasPlayed = true; // Помечаем, что звук уже проигран
            }
        }
    }
}
