using UnityEngine;

public class LaunchTrigger : MonoBehaviour
{
    public EscapePod escapePod;
    public TriggerButton launchButton;

    [Header("Primary Audio Settings")]
    public AudioSource launchSound;
    public AudioClip launchClip;
    public float soundDelay = 0f;
    [Range(0.1f, 3f)]
    public float playbackSpeed = 1f;
    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("Secondary Audio Settings")]
    public AudioSource secondarySound;
    public AudioClip secondaryClip;
    public float secondarySoundDelay = 0f;
    [Range(0.1f, 3f)]
    public float secondaryPlaybackSpeed = 1f;
    [Range(0f, 1f)]
    public float secondaryVolume = 1f;

    private bool alreadyLaunched = false;

    public AudioSource Ambient;

    [Header("Скрипты")]
    public ShowTheEnd showTheEnd;

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

        if (secondarySound != null)
        {
            secondarySound.playOnAwake = false;
            secondarySound.clip = secondaryClip;
            secondarySound.pitch = secondaryPlaybackSpeed;
            secondarySound.volume = secondaryVolume;
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
            if (soundDelay > 0f)
                Invoke(nameof(PlayLaunchSound), soundDelay);
            else
                PlayLaunchSound();
        }

        if (secondarySound != null && secondaryClip != null)
        {
            if (secondarySoundDelay > 0f)
                Invoke(nameof(PlaySecondarySound), secondarySoundDelay);
            else
                PlaySecondarySound();
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
        }

        escapePod.Launch();
        alreadyLaunched = true;
    }

    private void PlayLaunchSound()
    {
        if (launchSound != null)
        {
            launchSound.Play();
        }
    }

    private void PlaySecondarySound()
    {
        if (secondarySound != null)
        {
            Ambient.Stop();
            showTheEnd.AudioCons();
            secondarySound.Play();
        }
    }
}
