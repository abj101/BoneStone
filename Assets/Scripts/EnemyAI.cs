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

    [Header("Movement")]
    [Tooltip("If angle to target exceeds this (degrees), enemy stops and rotates in place like a tank")]
    public float tankTurnAngle = 90f;
    public float tankTurnSpeed = 10f;

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

        Vector3 targetPos = player.position + jitterOffset;
        Vector3 toTarget = targetPos - transform.position;
        toTarget.y = 0f;
        float toTargetMag = toTarget.sqrMagnitude;
        bool mustTurnInPlace = toTargetMag > 0.001f && Vector3.Angle(transform.forward, toTarget.normalized) > tankTurnAngle;

        if (mustTurnInPlace)
        {
            // Stay in place and rotate like a tank until within tankTurnAngle
            agent.SetDestination(transform.position);
            agent.isStopped = true;
            agent.updateRotation = false;
            transform.forward = Vector3.Lerp(transform.forward, toTarget.normalized, Time.deltaTime * tankTurnSpeed);
        }
        else
        {
            agent.updateRotation = true;
            agent.isStopped = false;
            agent.SetDestination(targetPos);
        }

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

        // Keep facing the dash direction so we don't drift/rotate while moving
        transform.forward = chargeDirection;
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
        agent.updateRotation = true;
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
        agent.updateRotation = false; // Rotate in place while aiming
    }

    void EnterCharge()
    {
        state = State.Charging;
        chargeDuration_elapsed = 0f;

        agent.ResetPath();
        agent.isStopped = false;
        agent.updateRotation = false; // Don't rotate during dash

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        chargeDirection = toPlayer.normalized;
        transform.forward = chargeDirection; // Lock facing to dash direction

        lastAttackTime = Time.time;
    }

    void EndCharge()
    {
        agent.velocity = Vector3.zero;
        agent.updateRotation = true;

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

        // Use both velocity (set during charge) and desiredVelocity (chase) so walk plays when moving and when dashing
        float speed = Mathf.Max(agent.velocity.magnitude, agent.desiredVelocity.magnitude);
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