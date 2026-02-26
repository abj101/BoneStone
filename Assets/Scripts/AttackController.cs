using UnityEngine;
using UnityEngine.InputSystem;

public class AttackController : MonoBehaviour
{
    [SerializeField] private InputActionReference attackAction;

    private WeaponHolder _weaponHolder;

    private void Awake()
    {
        _weaponHolder = GetComponent<WeaponHolder>();
    }

    private void OnEnable() => attackAction.action.Enable();
    private void OnDisable() => attackAction.action.Disable();

    private void Update()
    {
        if (attackAction.action.WasPressedThisFrame())
            _weaponHolder.Attack();
    }
}