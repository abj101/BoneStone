using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    private Collider _col;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        _col.enabled = false;
    }

    public void EnableHitbox() => _col.enabled = true;
    public void DisableHitbox() => _col.enabled = false;

    private void OnTriggerEnter(Collider other)
    {
        var health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }
}