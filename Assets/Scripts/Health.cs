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

    private float lastDamageTime;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;
    public event Action<Vector3> OnKnockback;

    void Awake()
    {
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(int amount, Vector3 damageSourcePosition, float knockbackForce = 5f)
    {
        if (IsDead) return;
        if (amount <= 0) return;

        // Invulnerability check
        if (Time.time < lastDamageTime + invulnerabilityDuration)
            return;

        lastDamageTime = Time.time;

        CurrentHealth -= amount;
        CurrentHealth = Mathf.Max(CurrentHealth, 0);

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        // Compute knockback direction
        Vector3 direction = (transform.position - damageSourcePosition).normalized;
        OnKnockback?.Invoke(direction * knockbackForce);

        if (CurrentHealth == 0)
        {
            Die();
        }
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
        if (destroyOnDeath)
            Destroy(gameObject);
    }
}