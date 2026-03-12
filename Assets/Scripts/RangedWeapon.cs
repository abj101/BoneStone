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

    [Header("Visual Bullet")]
    [SerializeField] private GameObject bulletVisualPrefab;
    [SerializeField] private float bulletSpeed = 25f;

    [Header("Hitscan Line")]
    [SerializeField] private float lineWidth = 0.06f;
    [SerializeField] private float lineDuration = 0.12f;
    [SerializeField] private Color lineColor = new Color(1f, 0.85f, 0.2f, 0.8f);
    [SerializeField] private float lineGroundOffset = 0.06f;

    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    private float _nextFireTime;

    public override void Attack()
    {
        if (Time.time < _nextFireTime) return;
        _nextFireTime = Time.time + Mathf.Max(0f, fireCooldown);

        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound, 0.2f);

        Transform owner = Owner != null ? Owner : transform;

        Vector3 origin = muzzle != null ? muzzle.position : owner.position + Vector3.up * 1f;
        Vector3 forward = owner.forward;

        Vector3 dir = new Vector3(forward.x, 0f, forward.z);
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        Vector3 endPoint = origin + dir * range;

        if (GroundHitboxUtility.RaycastOnGround(origin, forward, range, hitMask, out RaycastHit hit,
                ignoreRoot: Owner))
        {
            endPoint = hit.point;

            Health health = hit.collider.GetComponentInParent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage, owner.position);
            }
        }

        SpawnBulletVisual(origin, endPoint);
        SpawnHitscanLine(origin, endPoint);
    }

    private void SpawnBulletVisual(Vector3 origin, Vector3 target)
    {
        if (bulletVisualPrefab == null) return;

        GameObject bullet = Instantiate(bulletVisualPrefab, origin, Quaternion.identity);
        BulletVisual vis = bullet.GetComponent<BulletVisual>();
        if (vis != null)
            vis.Initialize(target, bulletSpeed);
        else
            Destroy(bullet, 2f);
    }

    private void SpawnHitscanLine(Vector3 origin, Vector3 endPoint)
    {
        Vector3 groundStart = new Vector3(origin.x, origin.y - 0.8f + lineGroundOffset, origin.z);
        Vector3 groundEnd = new Vector3(endPoint.x, origin.y - 0.8f + lineGroundOffset, endPoint.z);

        GameObject lineObj = new GameObject("HitscanLine");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.SetPosition(0, groundStart);
        lr.SetPosition(1, groundEnd);

        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.numCapVertices = 2;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = lineColor;
        mat.renderQueue = 2450;
        lr.material = mat;

        lr.startColor = lineColor;
        lr.endColor = lineColor;

        HitscanLineFade fader = lineObj.AddComponent<HitscanLineFade>();
        fader.Initialize(lr, lineDuration, lineColor);
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
