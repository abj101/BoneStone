using UnityEngine;
using UnityEngine.AI;

public class KnockbackController : MonoBehaviour
{
    [SerializeField] private float knockbackDecay = 8f;
    [SerializeField] private float minVelocityToReenableAgent = 0.5f;

    private Vector3 knockbackVelocity;
    private NavMeshAgent agent;
    private bool agentDisabledByKnockback;

    public Vector3 KnockbackVelocity => knockbackVelocity;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

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

        if (agentDisabledByKnockback)
        {
            if (knockbackVelocity.magnitude > minVelocityToReenableAgent)
            {
                // Apply horizontal-only movement so we don't sink through the ground
                Vector3 move = knockbackVelocity * Time.deltaTime;
                move.y = 0f;
                transform.position += move;
            }
            else
            {
                // Knockback finished: snap to NavMesh and re-enable agent
                knockbackVelocity = Vector3.zero;
                agentDisabledByKnockback = false;
                if (agent != null)
                {
                    if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                        transform.position = hit.position;
                    agent.enabled = true;
                    agent.Warp(transform.position);
                }
            }
        }
    }

    private void ApplyKnockback(Vector3 force)
    {
        knockbackVelocity = force;

        if (agent == null) return;

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.enabled = false;
            agentDisabledByKnockback = true;
        }
        else
        {
            // Agent already disabled (e.g. during charge); still apply knockback via flag so Update uses transform
            agentDisabledByKnockback = true;
        }
    }
}