using UnityEngine;

public class MeleeWeapon : Weapon
{
    [Header("Damage")]
    [SerializeField] private int damage = 25;

    [Header("Hitbox")]
    [SerializeField] private Collider hitbox;

    private Animator _animator;
    private bool _isAttacking;

    private void Awake()
    {
        if (hitbox != null)
            hitbox.enabled = false;

        _animator = GetComponent<Animator>();
    }

    public override void Attack()
    {
        if (_isAttacking) return;

        _isAttacking = true;
        _animator.SetTrigger("Attack");
    }

    // Called by Animation Event
    public void EnableHitbox()
    {
        if (hitbox != null)
            hitbox.enabled = true;
        Debug.Log("[MeleeWeapon] Hitbox enabled");
    }

    // Called by Animation Event
    public void DisableHitbox()
    {
        if (hitbox != null)
            hitbox.enabled = false;

        _isAttacking = false;
        Debug.Log("[MeleeWeapon] Hitbox disabled");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[MeleeWeapon] OnTriggerEnter: {other.gameObject.name}, _isAttacking={_isAttacking}");

        if (!_isAttacking) return;

        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            Debug.Log($"[MeleeWeapon] Dealing {damage} damage to {other.gameObject.name}");
            health.TakeDamage(damage, transform.position);
        }
    }
}