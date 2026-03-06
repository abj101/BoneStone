using UnityEngine;
using UnityEngine.InputSystem;

public class AttackController : MonoBehaviour
{
    [SerializeField] private InputActionReference leftAttackAction;
    [SerializeField] private InputActionReference rightAttackAction;

    private WeaponHolder _weaponHolder;

    private void Awake()
    {
        _weaponHolder = GetComponent<WeaponHolder>();
    }

    private void OnEnable()
    {
        if (leftAttackAction != null)
            leftAttackAction.action.Enable();

        if (rightAttackAction != null)
            rightAttackAction.action.Enable();
    }

    private void OnDisable()
    {
        if (leftAttackAction != null)
            leftAttackAction.action.Disable();

        if (rightAttackAction != null)
            rightAttackAction.action.Disable();
    }

    private void Update()
    {
        if (leftAttackAction != null && leftAttackAction.action.WasPressedThisFrame())
            _weaponHolder.AttackLeft();

        if (rightAttackAction != null && rightAttackAction.action.WasPressedThisFrame())
            _weaponHolder.AttackRight();
    }

    // Legacy single-attack input
    //
    // [SerializeField] private InputActionReference attackAction;
    //
    // private void OnEnable() => attackAction.action.Enable();
    // private void OnDisable() => attackAction.action.Disable();
    //
    // private void Update()
    // {
    //     if (attackAction.action.WasPressedThisFrame())
    //         _weaponHolder.Attack();
    // }
}