using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    [Header("Damage")]
    [SerializeField] private int damage = 25;

    [Header("Cone Hitbox")]
    [SerializeField] private float coneRadius = 2f;
    [SerializeField] private float coneAngle = 90f;
    [SerializeField] private float height = 1f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private bool hitTriggers = true;
    [SerializeField] private bool drawGizmos = true;

    [Header("Hitbox Visual")]
    [SerializeField] private Color hitboxColor = new Color(1f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private float hitboxFadeDuration = 0.12f;
    [SerializeField] private int coneSegments = 20;
    [SerializeField] private float groundOffset = 0.05f;
    [Header("Audio")]
    [SerializeField] private AudioClip[] swingClips;

    private Animator _animator;
    private bool _isAttacking;
    private bool _hitboxActive;
    private bool _hitboxFading;
    private float _fadeElapsed;
    private Material _hitboxMaterial;
    private readonly HashSet<int> _hitHealthIdsThisAttack = new HashSet<int>();
    private GameObject _activeVisual;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public override void Attack()
    {
        if (_isAttacking) return;

        _isAttacking = true;
        _hitHealthIdsThisAttack.Clear();

        if (swingClips !=  null && swingClips.Length > 0)
        {
            AudioClip clip = swingClips[Random.Range(0, swingClips.Length)];
            AudioSource.PlayClipAtPoint(clip, transform.position);
        } 
            

        if (_animator != null)
        {
            _animator.SetTrigger("Attack");
        }
        else
        {
            EnableHitbox();
            DisableHitbox();
        }
    }

    private void Update()
    {
        if (_hitboxFading)
        {
            _fadeElapsed += Time.deltaTime;
            float t = _fadeElapsed / hitboxFadeDuration;
            if (t >= 1f)
            {
                DestroyHitboxVisual();
                _hitboxFading = false;
                return;
            }
            if (_hitboxMaterial != null)
            {
                Color c = hitboxColor;
                c.a = hitboxColor.a * (1f - t);
                _hitboxMaterial.color = c;
            }
            if (_activeVisual != null)
                UpdateVisualTransform(_activeVisual.transform);
            return;
        }

        if (!_hitboxActive) return;

        if (_activeVisual != null)
            UpdateVisualTransform(_activeVisual.transform);

        DoDamageSweep();
    }

    // Called by Animation Event
    public void EnableHitbox()
    {
        if (!_isAttacking) return;
        _hitboxActive = true;
        SpawnHitboxVisual();
    }

    // Called by Animation Event
    public void DisableHitbox()
    {
        _hitboxActive = false;
        _isAttacking = false;
        if (_activeVisual != null && hitboxFadeDuration > 0f)
        {
            _hitboxFading = true;
            _fadeElapsed = 0f;
        }
        else
            DestroyHitboxVisual();
    }

    private void SpawnHitboxVisual()
    {
        DestroyHitboxVisual();

        _activeVisual = new GameObject("ConeHitboxVisual");
        MeshFilter mf = _activeVisual.AddComponent<MeshFilter>();
        MeshRenderer mr = _activeVisual.AddComponent<MeshRenderer>();

        mf.mesh = BuildConeMesh(coneRadius, coneAngle, coneSegments);

        _hitboxMaterial = new Material(Shader.Find("Sprites/Default"));
        _hitboxMaterial.color = hitboxColor;
        _hitboxMaterial.renderQueue = 2450;
        mr.material = _hitboxMaterial;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        UpdateVisualTransform(_activeVisual.transform);
    }

    private void UpdateVisualTransform(Transform visual)
    {
        Transform origin = Owner != null ? Owner : transform;
        Vector3 pos = origin.position;

        int ownerLayer = origin.gameObject.layer;
        int groundMask = ~(1 << ownerLayer);

        if (Physics.Raycast(new Vector3(pos.x, pos.y + 0.5f, pos.z), Vector3.down, out RaycastHit groundHit, 10f, groundMask, QueryTriggerInteraction.Ignore))
            pos.y = groundHit.point.y + groundOffset;
        else
            pos.y = pos.y - 1f + groundOffset;

        visual.position = pos;
        visual.rotation = Quaternion.Euler(0f, origin.eulerAngles.y, 0f);
        visual.localScale = Vector3.one;
    }

    private static Mesh BuildConeMesh(float radius, float angleDeg, int segments)
    {
        float halfAngle = angleDeg * 0.5f * Mathf.Deg2Rad;
        int vertCount = segments + 2;
        var verts = new Vector3[vertCount];
        var tris = new int[segments * 3];

        verts[0] = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
            verts[i + 1] = new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
        }

        for (int i = 0; i < segments; i++)
        {
            tris[i * 3] = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = i + 2;
        }

        var mesh = new Mesh { vertices = verts, triangles = tris };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void DestroyHitboxVisual()
    {
        if (_activeVisual != null)
        {
            Destroy(_activeVisual);
            _activeVisual = null;
        }
        _hitboxMaterial = null;
    }

    private void DoDamageSweep()
    {
        Transform origin = Owner != null ? Owner : transform;
        QueryTriggerInteraction qti = hitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

        Collider[] hits = Physics.OverlapSphere(origin.position, coneRadius, hitMask, qti);

        if (hits == null || hits.Length == 0) return;

        float halfAngle = coneAngle * 0.5f;
        Vector3 forward = origin.forward;
        forward.y = 0f;
        forward.Normalize();

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i];
            if (col == null) continue;

            if (Owner != null && col.transform.IsChildOf(Owner)) continue;

            Vector3 toTarget = col.transform.position - origin.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f) continue;

            if (Vector3.Angle(forward, toTarget) > halfAngle) continue;

            Health health = col.GetComponentInParent<Health>();
            if (health == null) continue;

            int id = health.GetInstanceID();
            if (_hitHealthIdsThisAttack.Contains(id)) continue;

            _hitHealthIdsThisAttack.Add(id);
            health.TakeDamage(damage, origin.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Transform origin = Owner != null ? Owner : transform;
        Gizmos.color = Color.red;

        float halfAngle = coneAngle * 0.5f;
        Vector3 forward = origin.forward;
        Vector3 leftEdge = Quaternion.Euler(0, -halfAngle, 0) * forward * coneRadius;
        Vector3 rightEdge = Quaternion.Euler(0, halfAngle, 0) * forward * coneRadius;

        Gizmos.DrawLine(origin.position, origin.position + leftEdge);
        Gizmos.DrawLine(origin.position, origin.position + rightEdge);

        int segs = 20;
        Vector3 prev = origin.position + leftEdge;
        for (int i = 1; i <= segs; i++)
        {
            float t = (float)i / segs;
            float a = Mathf.Lerp(-halfAngle, halfAngle, t);
            Vector3 pt = origin.position + Quaternion.Euler(0, a, 0) * forward * coneRadius;
            Gizmos.DrawLine(prev, pt);
            prev = pt;
        }
    }

    private void OnDisable()
    {
        DestroyHitboxVisual();
        _hitboxActive = false;
        _hitboxFading = false;
        _isAttacking = false;
    }
}