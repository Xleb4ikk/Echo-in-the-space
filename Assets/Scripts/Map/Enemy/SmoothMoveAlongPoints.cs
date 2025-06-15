using UnityEngine;

public class MoveAlongPointsWithAnimationAndSound : MonoBehaviour
{
    [Header("Путь движения")]
    public Transform[] points;
    public float moveSpeed = 2f;
    public float rotateSpeed = 5f;

    [Header("Зона активации")]
    public Collider triggerZone;
    public Transform playerTransform;
    public GameObject TrigerObject;

    [Header("Анимация и звук")]
    public Animator animator;             
    public AudioSource footstepAudio;    
    [Range(0.5f, 2f)]
    public float pitch = 1f;
    public float stepDelay = 0.5f;

    [Header("Монстр")]
    public GameObject Monster;
    public CapsuleCollider MonsterColider;

    private int currentIndex = 0;
    private bool isMoving = false;

    private float stepTimer = 0f;


    void Update()
    {
        if (!isMoving && triggerZone != null && playerTransform != null)
        {
            bool isPlayerInside = triggerZone.bounds.Contains(playerTransform.position);

            if (isPlayerInside)
            {
                StartMoving();
                TrigerObject.SetActive(false);
                
            }
        }

        if (!isMoving || points.Length == 0)
        {
            StopFootstepsAndAnimation();
            return;
        }

        MoveAndRotate();

        PlayFootstepSoundWithDelay();

        if (!animator.GetBool("Injured"))
            animator.SetBool("Injured", true);
    }

    private void MoveAndRotate()
    {
        Transform target = points[currentIndex];
        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;

        if (distance < 0.1f)
        {
            currentIndex++;
            if (currentIndex >= points.Length)
            {
                isMoving = false;
                StopFootstepsAndAnimation();
                return;
            }
            target = points[currentIndex];
            direction = target.position - transform.position;
        }

        Vector3 moveDir = direction.normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
    }

    private void PlayFootstepSoundWithDelay()
    {
        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            if (!footstepAudio.isPlaying)
            {
                footstepAudio.pitch = pitch;
                footstepAudio.Play();
            }
            stepTimer = stepDelay;
        }
    }

    private void StopFootstepsAndAnimation()
    {
        if (footstepAudio.isPlaying)
            footstepAudio.Stop();

        if (animator != null)
            animator.SetBool("Injured", false);

    }

    public void StartMoving()
    {
        Monster.SetActive(true);
        MonsterColider.enabled = true;
        isMoving = true;
        currentIndex = 0;
        stepTimer = 0f;
    }
}
