using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private Health playerHealth;

    [Header("Combat")]
    public int damage = 10;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public float knockbackForce = 6f;

    private float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<Health>();
    }

    void Update()
    {
        if (player == null) return;

        agent.SetDestination(player.position);

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            playerHealth.TakeDamage(
                damage,
                transform.position,
                knockbackForce
            );

            lastAttackTime = Time.time;
        }
    }
}