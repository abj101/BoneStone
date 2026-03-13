using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private Health playerHealth;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private RuntimeAnimatorController enemyController;
    [SerializeField] private float moveBlendDampTime = 0.1f;

    private static readonly int BlendHash = Animator.StringToHash("movement");

    [Header("Detection")]
    public float awarenessRadius = 12f;
    public float jitterRadius = 1.5f;
    public float reaggroCooldown = 2f;

    [Header("Combat")]
    public int damage = 10;
    public float attackRange = 5f;
    public float attackCooldown = 2f;
    public float chargeDuration = 0.4f;
    public float chargeSpeed = 14f;
    public float knockbackForce = 6f;
    public LayerMask playerLayer;

    private float lastAttackTime;
    private float lastDeaggroTime = -999f;
    private float chargeDuration_elapsed;
    private Vector3 chargeDirection;
    private Vector3 jitterOffset;
    private float jitterRefreshTimer;
    private float _baseOffset;

    private float _currentBlend;
    private float _blendVel;

    private enum State { Idle, Chase, ChargeWindup, Charging }
    private State state = State.Idle;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        _baseOffset = agent != null ? agent.baseOffset : 0f;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null && animator.runtimeAnimatorController == null && enemyController != null)
            animator.runtimeAnimatorController = enemyController;

        var go = GameObject.FindGameObjectWithTag("Player");
        player = go.transform;
        playerHealth = go.GetComponent<Health>();
        RefreshJitter();
    }

    void Update()
    {
        if (player == null) return;
        if (agent != null && !agent.enabled) return;

        switch (state)
        {
            case State.Idle:         UpdateIdle();     break;
            case State.Chase:        UpdateChase();    break;
            case State.ChargeWindup: UpdateWindup();   break;
            case State.Charging:     UpdateCharging(); break;
        }

        UpdateAnimation();
    }

    void UpdateIdle()
    {
        bool cooldownExpired = Time.time >= lastDeaggroTime + reaggroCooldown;
        bool inRange = Vector3.Distance(transform.position, player.position) <= awarenessRadius;

        if (inRange && cooldownExpired)
            EnterChase();
    }

    void UpdateChase()
    {
        jitterRefreshTimer -= Time.deltaTime;
        if (jitterRefreshTimer <= 0f)
            RefreshJitter();

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > awarenessRadius)
        {
            EnterIdle();
            return;
        }

        agent.SetDestination(player.position + jitterOffset);

        if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            EnterWindup();
    }

    void UpdateWindup()
    {
        agent.SetDestination(transform.position);

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.001f)
        {
            transform.forward = Vector3.Lerp(
                transform.forward,
                toPlayer.normalized,
                Time.deltaTime * 10f
            );
        }

        chargeDuration_elapsed += Time.deltaTime;
        if (chargeDuration_elapsed >= 0.2f)
            EnterCharge();
    }

    void UpdateCharging()
    {
        chargeDuration_elapsed += Time.deltaTime;

        agent.velocity = chargeDirection * chargeSpeed;

        Vector3 feetPos = transform.position - Vector3.up * _baseOffset;
        Vector3 step = chargeDirection * chargeSpeed * Time.deltaTime;
        if (NavMesh.Raycast(feetPos, feetPos + step, out NavMeshHit wallHit, NavMesh.AllAreas))
        {
            EndCharge();
            return;
        }

        if (Physics.OverlapSphere(transform.position, 0.8f, playerLayer).Length > 0)
        {
            playerHealth.TakeDamage(damage, transform.position, knockbackForce);
            EndCharge();
            return;
        }

        if (chargeDuration_elapsed >= chargeDuration)
            EndCharge();
    }

    void EnterChase()
    {
        state = State.Chase;
        agent.isStopped = false;
    }

    void EnterIdle()
    {
        state = State.Idle;
        lastDeaggroTime = Time.time;
        agent.SetDestination(transform.position);
        agent.isStopped = true;
    }

    void EnterWindup()
    {
        state = State.ChargeWindup;
        chargeDuration_elapsed = 0f;
        agent.isStopped = true;
    }

    void EnterCharge()
    {
        state = State.Charging;
        chargeDuration_elapsed = 0f;

        agent.ResetPath();
        agent.isStopped = false;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        chargeDirection = toPlayer.normalized;

        lastAttackTime = Time.time;
    }

    void EndCharge()
    {
        agent.velocity = Vector3.zero;

        if (!agent.isOnNavMesh)
        {
            Vector3 feetPos = transform.position - Vector3.up * _baseOffset;
            if (NavMesh.SamplePosition(feetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                transform.position = new Vector3(hit.position.x, hit.position.y + _baseOffset, hit.position.z);
                agent.Warp(transform.position);
            }
        }

        state = State.Chase;
    }

    void UpdateAnimation()
    {
        if (animator == null || agent == null) return;
        if (animator.runtimeAnimatorController == null) return;

        // Use agent speed to drive Blend (0 = idle, 1 = moving)
        float speed = agent.velocity.magnitude;
        float targetBlend = speed > 0.1f ? 1f : 0f;

        _currentBlend = Mathf.SmoothDamp(_currentBlend, targetBlend, ref _blendVel, moveBlendDampTime);
        animator.SetFloat(BlendHash, _currentBlend);
    }

    public void InterruptCharge()
    {
        if (state == State.Charging)
            EndCharge();
    }

    void RefreshJitter()
    {
        Vector2 rand = Random.insideUnitCircle * jitterRadius;
        jitterOffset = new Vector3(rand.x, 0f, rand.y);
        jitterRefreshTimer = Random.Range(1.5f, 3f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, awarenessRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}