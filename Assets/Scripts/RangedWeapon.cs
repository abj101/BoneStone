using UnityEngine;

public class RangedWeapon : Weapon
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;

    [Header("Hitscan")]
    [SerializeField] private float range = 12f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Firing")]
    [SerializeField] private float fireCooldown = 0.15f;
    [SerializeField] private Transform muzzle;

    [Header("Visual Bullet (placeholder)")]
    [SerializeField] private GameObject bulletVisualPrefab;
    [SerializeField] private float bulletSpeed = 25f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    private float _nextFireTime;

    public override void Attack()
    {
        if (Time.time < _nextFireTime) return;
        _nextFireTime = Time.time + Mathf.Max(0f, fireCooldown);

        Transform owner = Owner != null ? Owner : transform;

        Vector3 origin = muzzle != null ? muzzle.position : owner.position + Vector3.up * 1f;
        Vector3 forward = owner.forward;

        Vector3 dir = new Vector3(forward.x, 0f, forward.z);
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        Vector3 targetPoint = origin + dir * range;

        if (GroundHitboxUtility.RaycastOnGround(origin, owner.forward, range, hitMask, out RaycastHit hit))
        {
            targetPoint = hit.point;

            Health health = hit.collider != null ? hit.collider.GetComponentInParent<Health>() : null;
            if (health != null && (Owner == null || !health.transform.IsChildOf(Owner)))
            {
                health.TakeDamage(damage, owner.position);
            }
        }

        SpawnBulletVisual(origin, targetPoint);
    }

    private void SpawnBulletVisual(Vector3 origin, Vector3 target)
    {
        if (bulletVisualPrefab == null) return;

        GameObject bullet = Object.Instantiate(bulletVisualPrefab, origin, Quaternion.identity);

        BulletVisual vis = bullet.GetComponent<BulletVisual>();
        if (vis != null)
        {
            vis.Initialize(target, bulletSpeed);
        }
        else
        {
            // Fallback: just destroy after a moment so missing component doesn't spam the scene.
            Object.Destroy(bullet, 2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Transform owner = Owner != null ? Owner : transform;
        Vector3 origin = muzzle != null ? muzzle.position : owner.position + Vector3.up * 1f;

        Vector3 forward = owner.forward;
        Vector3 dir = new Vector3(forward.x, 0f, forward.z);
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + dir * range);
    }
}

