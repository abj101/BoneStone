using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private int _current;

    private void Awake()
    {
        _current = maxHealth;
    }

    public void TakeDamage(int dmg)
    {
        _current -= dmg;

        if (_current <= 0)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}