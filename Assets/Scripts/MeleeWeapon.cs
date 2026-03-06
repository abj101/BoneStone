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

    [Header("Hitbox Visual")]
    [Tooltip("Assign a prefab: cube mesh, no Collider, transparent/emissive material. If null, a default cube is used.")]
    [SerializeField] private GameObject hitboxVisualPrefab;
    [SerializeField] private Color hitboxColor = new Color(1f, 0.2f, 0.2f, 0.35f);

    private Animator _animator;
    private bool _isAttacking;
    private bool _hitboxActive;
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
        DestroyHitboxVisual();
    }

    private void SpawnHitboxVisual()
    {
        DestroyHitboxVisual();

        Transform origin = Owner != null ? Owner : transform;

        if (hitboxVisualPrefab != null)
        {
            _activeVisual = Instantiate(hitboxVisualPrefab, origin);
        }
        else
        {
            _activeVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(_activeVisual.GetComponent<Collider>());

            Renderer rend = _activeVisual.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.SetFloat("_Mode", 3f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                mat.color = hitboxColor;
                rend.material = mat;
            }

            _activeVisual.transform.SetParent(origin);
        }

        UpdateVisualTransform(_activeVisual.transform);
    }

    private void UpdateVisualTransform(Transform visual)
    {
        Transform origin = Owner != null ? Owner : transform;

        visual.position = origin.TransformPoint(localOffset);
        visual.rotation = Quaternion.Euler(0f, origin.eulerAngles.y, 0f);
        visual.localScale = new Vector3(sizeXZ.x, height, sizeXZ.y);
    }

    private void DestroyHitboxVisual()
    {
        if (_activeVisual != null)
        {
            Destroy(_activeVisual);
            _activeVisual = null;
        }
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
        _isAttacking = false;
    }
}