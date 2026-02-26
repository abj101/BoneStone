using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private SwordHitbox _hitbox;
    [SerializeField] private float _attackCooldown = 0.4f;

    private bool _canAttack = true;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && _canAttack)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        _canAttack = false;

        _hitbox.EnableHitbox();

        yield return new WaitForSeconds(0.15f);

        _hitbox.DisableHitbox();

        yield return new WaitForSeconds(_attackCooldown);

        _canAttack = true;
    }
}