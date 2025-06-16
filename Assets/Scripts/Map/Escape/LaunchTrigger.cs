using UnityEngine;

public class LaunchTrigger : MonoBehaviour
{
    public EscapePod escapePod;
    public TriggerButton launchButton;

    [Header("Audio Settings")]
    public AudioSource launchSound;       // Компонент AudioSource
    public AudioClip launchClip;          // Аудиофайл
    public float soundDelay = 0f;         // Задержка воспроизведения
    [Range(0.1f, 3f)]
    public float playbackSpeed = 1f;      // Скорость звука
    [Range(0f, 1f)]
    public float volume = 1f;             // Громкость звука

    private bool alreadyLaunched = false;

    void Start()
    {
        if (launchButton != null)
        {
            launchButton.OnButtonPressed += HandleButtonPress;
        }

        if (launchSound != null)
        {
            launchSound.playOnAwake = false;
            launchSound.clip = launchClip;
            launchSound.pitch = playbackSpeed;
            launchSound.volume = volume;
        }
    }

    void OnDestroy()
    {
        if (launchButton != null)
        {
            launchButton.OnButtonPressed -= HandleButtonPress;
        }
    }

    private void HandleButtonPress()
    {
        if (alreadyLaunched) return;

        if (launchSound != null && launchClip != null)
        {
            launchSound.pitch = playbackSpeed;
            launchSound.volume = volume;

            if (soundDelay > 0f)
                Invoke(nameof(PlayLaunchSound), soundDelay);
            else
                PlayLaunchSound();
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.parent = escapePod.transform;

            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.useGravity = false;
                playerRb.isKinematic = true;
                playerRb.linearVelocity = Vector3.zero;
            }

            escapePod.Launch();
            alreadyLaunched = true;
        }
    }

    private void PlayLaunchSound()
    {
        if (launchSound != null && launchClip != null)
        {
            launchSound.Play();
        }
    }
}
