using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    [Header("Damage")]
    [SerializeField] private int damage = 25;

    [Header("Ground Hitbox (Hades-style)")]
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 0f, 1f);
    [SerializeField] private Vector2 sizeXZ = new Vector2(1.2f, 1.2f);
    [SerializeField] private float height = 1f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private bool hitTriggers = true;
    [SerializeField] private bool drawGizmos = true;

    [Header("Hitbox Visual (matches bullet/hitscan style)")]
    [Tooltip("Assign a prefab: flat quad mesh, no Collider, transparent material. If null, a default quad is created.")]
    [SerializeField] private GameObject hitboxVisualPrefab;
    [SerializeField] private Color hitboxColor = new Color(1f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private float hitboxFadeDuration = 0.12f;
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

        _activeVisual = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(_activeVisual.GetComponent<Collider>());

        Renderer rend = _activeVisual.GetComponent<Renderer>();
        if (rend != null)
        {
            _hitboxMaterial = new Material(Shader.Find("Sprites/Default"));
            _hitboxMaterial.color = hitboxColor;
            _hitboxMaterial.renderQueue = 2450;
            rend.material = _hitboxMaterial;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
        }

        UpdateVisualTransform(_activeVisual.transform);
    }

    private void UpdateVisualTransform(Transform visual)
    {
        Transform origin = Owner != null ? Owner : transform;

        Vector3 center = origin.TransformPoint(localOffset);

        if (Physics.Raycast(new Vector3(center.x, center.y + 2f, center.z), Vector3.down, out RaycastHit groundHit, 10f))
            center.y = groundHit.point.y + 0.05f;
        else
            center.y = origin.position.y - 1f + 0.05f;

        visual.position = center;
        visual.rotation = Quaternion.Euler(90f, origin.eulerAngles.y, 0f);
        visual.localScale = new Vector3(sizeXZ.x, sizeXZ.y, 1f);
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

        Collider[] hits = GroundHitboxUtility.OverlapBoxOnGround(
            origin,
            localOffset,
            sizeXZ,
            height,
            hitMask,
            qti
        );

        if (hits == null || hits.Length == 0) return;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i];
            if (col == null) continue;

            if (Owner != null && col.transform.IsChildOf(Owner)) continue;

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
        GroundHitboxUtility.DrawGizmoBoxOnGround(origin, localOffset, sizeXZ, height);
    }

    private void OnDisable()
    {
        DestroyHitboxVisual();
        _hitboxActive = false;
        _hitboxFading = false;
        _isAttacking = false;
    }
}