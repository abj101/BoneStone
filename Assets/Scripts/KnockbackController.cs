using UnityEngine;
using UnityEngine.AI;

public class KnockbackController : MonoBehaviour
{
    [SerializeField] private float knockbackDecay = 8f;

    private Vector3 knockbackVelocity;
    private NavMeshAgent agent;

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

        if (agent != null && knockbackVelocity.magnitude > 0.1f)
        {
            agent.Move(knockbackVelocity * Time.deltaTime);
        }
    }

    private void ApplyKnockback(Vector3 force)
    {
        knockbackVelocity = force;

        if (agent != null)
            agent.ResetPath();
    }
}