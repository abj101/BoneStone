using UnityEngine;
using UnityEngine.AI;

public class KnockbackController : MonoBehaviour
{
    [SerializeField] private float knockbackDecay = 8f;
    [SerializeField] private float minVelocityToReenableAgent = 0.5f;

    private Vector3 knockbackVelocity;
    private NavMeshAgent agent;
    private EnemyAI enemyAI;
    private bool agentDisabledByKnockback;

    public Vector3 KnockbackVelocity => knockbackVelocity;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAI = GetComponent<EnemyAI>();

        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.OnKnockback += ApplyKnockback;
        }
    }

    void Update()
    {
        knockbackVelocity = Vector3.Lerp(
            knockbackVelocity,
            Vector3.zero,
            knockbackDecay * Time.deltaTime
        );

        if (!agentDisabledByKnockback) return;

        if (knockbackVelocity.magnitude > minVelocityToReenableAgent)
        {
            if (agent != null && agent.enabled)
            {
                Vector3 vel = knockbackVelocity;
                vel.y = 0f;
                agent.velocity = vel;
            }
        }
        else
        {
            knockbackVelocity = Vector3.zero;
            agentDisabledByKnockback = false;

            if (agent != null && agent.enabled)
                agent.velocity = Vector3.zero;
        }
    }

    private void ApplyKnockback(Vector3 force)
    {
        if (agent == null) return;

        knockbackVelocity = force;

        if (enemyAI != null)
            enemyAI.InterruptCharge();

        if (agent.enabled && agent.isOnNavMesh)
            agent.ResetPath();

        agentDisabledByKnockback = true;
    }
}
