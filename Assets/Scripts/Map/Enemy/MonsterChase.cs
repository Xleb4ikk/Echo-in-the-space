using UnityEngine;
using UnityEngine.AI;

public class MonsterChase : MonoBehaviour
{
    public Transform player;
    public float chaseRange = 10f;
    public float stopChaseDistance = 15f;
    public bool isEnabled = true;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isChasing = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isEnabled || player == null || agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= chaseRange)
        {
            isChasing = true;
        }

        if (distanceToPlayer > stopChaseDistance)
        {
            isChasing = false;
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.ResetPath();
            }
        }

        if (isChasing)
        {
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(player.position);
            }
        }

        if (animator != null)
        {
            animator.SetBool("isChasing", isChasing);
        }
    }

    public void EnableChase()
    {
        isEnabled = true;
    }

    public void DisableChase()
    {
        isEnabled = false;
        isChasing = false;

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }

        if (animator != null)
        {
            animator.SetBool("isChasing", false);
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
