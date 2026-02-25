using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class SwordWeapon : Weapon
{
    [Header("Combat")]
    [SerializeField] private int damage = 25;
    [SerializeField] private float attackDuration = 0.3f;
    [SerializeField] private float swingAngle = 120f;
    [SerializeField] private float swingSpeed = 720f;
    [SerializeField] private Transform pivot;

    private Collider _hitbox;
    private bool _isAttacking;

    private void Awake()
    {
        _hitbox = GetComponent<Collider>();
        _hitbox.isTrigger = true;
        _hitbox.enabled = false;
    }

    public override void Attack()
    {
        if (_isAttacking) return;
        StartCoroutine(Swing());
    }

    private IEnumerator Swing()
    {
        _isAttacking = true;
        _hitbox.enabled = true;

        float timer = 0f;
        float halfDuration = attackDuration / 2f;

        Quaternion startRot = pivot.localRotation;
        Quaternion left = startRot * Quaternion.Euler(0, -swingAngle / 2f, 0);
        Quaternion right = startRot * Quaternion.Euler(0, swingAngle / 2f, 0);

        // Windup
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            pivot.localRotation = Quaternion.Lerp(startRot, left, timer / halfDuration);
            yield return null;
        }

        // Swing through
        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            pivot.localRotation = Quaternion.Lerp(left, right, timer / halfDuration);
            yield return null;
        }

        _hitbox.enabled = false;
        pivot.localRotation = startRot;

        _isAttacking = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_owner == null) return;

        // Ignore self
        if (other.gameObject == _owner) return;

        // Also ignore anything that is a child of owner
        if (other.transform.IsChildOf(_owner.transform)) return;

        Health health = other.GetComponent<Health>();

        if (health != null)
        {
            health.TakeDamage(damage, transform.position);
        }
    }
}