using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invulnerabilityDuration = 0.5f;
    [SerializeField] private bool destroyOnDeath = true;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;
    public bool IsInvulnerable => _forceInvulnerable || Time.time < _lastDamageTime + invulnerabilityDuration;

    private float _lastDamageTime = -999f;
    private bool _forceInvulnerable;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;
    public event Action<Vector3> OnKnockback;

    private void Awake()
    {
        CurrentHealth = maxHealth;
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

    private void Die()
    {
        OnDeath?.Invoke();
        if (destroyOnDeath) Destroy(gameObject);
    }
}