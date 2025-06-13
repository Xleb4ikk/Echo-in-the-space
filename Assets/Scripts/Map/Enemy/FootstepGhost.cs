using UnityEngine;

public class FootstepGhost : MonoBehaviour
{
    public Transform player;                     // Игрок, на которого ориентируемся
    public Vector3[] pathPoints;                 // Путь призрака
    public float moveSpeed = 2f;                 // Скорость передвижения
    public float disappearViewAngle = 35f;       // Угол обзора игрока, при котором призрак исчезает

    [Header("Звуки шагов")]
    [SerializeField] private AudioClip regularFootstepSound;
    [SerializeField] private AudioClip stairsFootstepSound;
    [SerializeField, Range(0f, 1f)] private float ghostVolume = 0.7f;
    [SerializeField] private float ghostPitch = 1.0f;

    private int currentPathIndex = 0;
    private bool isActive = true;
    private AudioSource audioSource;

    private bool isOnStairs = false;
    private float stepTimer = 0f;
    private float stepInterval = 0.5f;

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
        isActive = true;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = ghostVolume;
        audioSource.pitch = ghostPitch;
    }

    void Update()
    {
        if (!isActive || player == null || pathPoints == null || pathPoints.Length == 0) return;

        MoveAlongPath();
        CheckPlayerView();
        HandleFootstepSound();
    }

    private void MoveAlongPath()
    {
        if (currentPathIndex >= pathPoints.Length)
        {
            StopAndDestroy();
            return;
        }

        Vector3 target = pathPoints[currentPathIndex];
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            currentPathIndex++;

            if (currentPathIndex >= pathPoints.Length)
            {
                StopAndDestroy();
            }
        }
    }

    private void HandleFootstepSound()
    {
        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            stepTimer = stepInterval;

            AudioClip clipToPlay = isOnStairs ? stairsFootstepSound : regularFootstepSound;

            if (clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
        }
    }

    private void CheckPlayerView()
    {
        Vector3 toGhost = (transform.position - player.position).normalized;
        float angle = Vector3.Angle(player.forward, toGhost);

        if (angle < disappearViewAngle)
        {
            StopAndDestroy();
        }
    }

    private void StopAndDestroy()
    {
        isActive = false;

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // Отключаем физику и анимацию, если есть
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.enabled = false;
        }

        Destroy(gameObject, 0.2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Stairs"))
        {
            isOnStairs = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Stairs"))
        {
            isOnStairs = false;
        }
    }
}
