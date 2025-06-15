using UnityEngine;

public class EventWalking : MonoBehaviour
{
    [Header("Настройки монстра")]
    [SerializeField] private GameObject Monster;
    [SerializeField] private Transform MonsterTransform;
    [SerializeField] private Animator MonsterAnimation;
    [SerializeField] private float MonsterSpeed = 5f;
    [SerializeField] private float MoveDistance = 5f;

    [Header("Звук шагов")]
    [SerializeField] private AudioClip FootstepClip;                // Один короткий звук шага
    [SerializeField] private AudioSource FootstepAudioSource;      // Источник звука
    [SerializeField] private float StepInterval = 0.5f;             // Интервал между шагами в секундах

    [Header("Триггер")]
    [SerializeField] private BoxCollider Trigger;
    [SerializeField] private Transform Player;

    public bool IsPlayerInside { get; private set; }

    private bool isMoving = false;
    private bool hasStarted = false;
    private Vector3 startPosition;

    private float stepTimer = 0f;

    private void Update()
    {
        CheckPlayerTrigger();

        if (isMoving)
        {
            MoveMonster();
            HandleFootsteps();
        }
    }

    private void CheckPlayerTrigger()
    {
        if (Trigger == null || Player == null || hasStarted)
            return;

        IsPlayerInside = Trigger.bounds.Contains(Player.position);

        if (IsPlayerInside)
        {
            StartMonsterMovement();
        }
    }

    public void StartMonsterMovement()
    {
        MonsterAnimation.SetBool("Walking", true);
        hasStarted = true;
        isMoving = true;
        startPosition = MonsterTransform.position;

        Debug.Log("Монстр начал движение");  
    }

    private void MoveMonster()
    {
        float distanceMoved = Vector3.Distance(startPosition, MonsterTransform.position);

        if (distanceMoved < MoveDistance)
        {
            MonsterTransform.Translate(Vector3.forward * Time.deltaTime * MonsterSpeed);
        }
        else
        {
            StopMonsterMovement();
        }
    }

    private void StopMonsterMovement()
    {
        isMoving = false;
        Debug.Log("Монстр остановился");

        MonsterAnimation.SetBool("Walking", false);
        Monster.SetActive(false);
    }

    private void HandleFootsteps()
    {
        if (FootstepClip == null || FootstepAudioSource == null)
            return;

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            FootstepAudioSource.PlayOneShot(FootstepClip);
            stepTimer = StepInterval;
        }
    }
}
