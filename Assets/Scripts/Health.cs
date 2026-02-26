using UnityEngine;
using System;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invulnerabilityDuration = 0.5f;
    [SerializeField] private bool destroyOnDeath = true;

    [Header("Damage Flash")]
    [SerializeField] private float flashDuration = 0.12f;
    [SerializeField] private Color flashColor = Color.red;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;
    public bool IsInvulnerable => _forceInvulnerable || Time.time < _lastDamageTime + invulnerabilityDuration;

    private float _lastDamageTime = -999f;
    private bool _forceInvulnerable;

    private Renderer[] _renderers;
    private MaterialPropertyBlock _propBlock;
    private readonly int _baseColorID = Shader.PropertyToID("_BaseColor"); // URP

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;
    public event Action<Vector3> OnKnockback;

    private void Awake()
    {
        CurrentHealth = maxHealth;

        _renderers = GetComponentsInChildren<Renderer>();
        _propBlock = new MaterialPropertyBlock();

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void SetInvulnerable(bool state) => _forceInvulnerable = state;

    public void TakeDamage(int amount, Vector3 damageSourcePosition, float knockbackForce = 5f)
    {
        if (IsDead) return;
        if (amount <= 0) return;
        if (IsInvulnerable) return;

        _lastDamageTime = Time.time;

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        Flash(); // 🔥 damage visual

        Vector3 direction = (transform.position - damageSourcePosition).normalized;
        OnKnockback?.Invoke(direction * knockbackForce);

        if (CurrentHealth == 0) Die();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        if (amount <= 0) return;

        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private void Flash()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // Apply flash color
        foreach (var rend in _renderers)
        {
            rend.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(_baseColorID, flashColor);
            rend.SetPropertyBlock(_propBlock);
        }

        yield return new WaitForSeconds(flashDuration);

        // Reset
        foreach (var rend in _renderers)
        {
            rend.GetPropertyBlock(_propBlock);
            _propBlock.Clear();
            rend.SetPropertyBlock(_propBlock);
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
        if (destroyOnDeath) Destroy(gameObject);
    }
}