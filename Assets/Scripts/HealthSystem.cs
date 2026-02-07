using System.Collections;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private float _knockbackDistance = 0.6f;
    [SerializeField] private float _knockbackDuration = 0.12f;
    [SerializeField] private LayerMask _damageLayers = 6;

    private int _currentHealth;
    private bool _isAlive = true;

    public int CurrentHealth => _currentHealth;
    public bool IsAlive => _isAlive;
    public bool IsBeingKnockedBack { get; private set; }

    private void Start()
    {
        _currentHealth = _maxHealth;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & _damageLayers) == 0)
            return;

        var dir = (transform.position - collision.contacts[0].point).normalized;
        dir.y = 0;
        TakeDamage(10, dir);
    }

    public void TakeDamage(int amount, Vector3 knockbackDir = default)
    {
        if (!_isAlive) return;

        _currentHealth = Mathf.Max(0, _currentHealth - amount);

        if (knockbackDir != Vector3.zero)
            StartCoroutine(SmoothKnockback(knockbackDir));

        if (_currentHealth <= 0)
        {
            _isAlive = false;
            gameObject.SetActive(false);
        }
    }

    private IEnumerator SmoothKnockback(Vector3 dir)
    {
        IsBeingKnockedBack = true;
        var start = transform.position;
        var end = start + dir * _knockbackDistance;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / _knockbackDuration;
            float smooth = 1f - (1f - t) * (1f - t); // ease out
            transform.position = Vector3.Lerp(start, end, smooth);
            yield return null;
        }

        IsBeingKnockedBack = false;
    }
}
