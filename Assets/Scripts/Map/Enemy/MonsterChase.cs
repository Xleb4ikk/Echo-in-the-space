using UnityEngine;
using UnityEngine.AI;

public class MonsterChase : MonoBehaviour
{
    public Transform player;
    public float chaseRange = 10f;
    public float stopChaseDistance = 15f;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= chaseRange)
        {
            isChasing = true;
        }

        if (distanceToPlayer > stopChaseDistance)
        {
            isChasing = false;
            agent.ResetPath();
        }

        if (isChasing)
        {
            agent.SetDestination(player.position);
        }

        // Устанавливаем параметр анимации
        if (animator != null)
        {
            animator.SetBool("isChasing", isChasing);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopChaseDistance);
    }
}
