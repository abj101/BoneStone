using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [Header("Sockets")]
    [SerializeField] private Transform leftWeaponSocket;
    [SerializeField] private Transform rightWeaponSocket;

    [Header("Default Weapons")]
    [SerializeField] private Weapon leftDefaultWeapon;
    [SerializeField] private Weapon rightDefaultWeapon;

    private Weapon _currentLeftWeapon;
    private Weapon _currentRightWeapon;

    private void Start()
    {
        if (leftDefaultWeapon != null)
            EquipLeft(leftDefaultWeapon);

        if (rightDefaultWeapon != null)
            EquipRight(rightDefaultWeapon);
    }

    public void EquipLeft(Weapon weaponPrefab)
    {
        if (leftWeaponSocket == null || weaponPrefab == null)
            return;

        if (_currentLeftWeapon != null)
            Destroy(_currentLeftWeapon.gameObject);

        Weapon newWeapon = Instantiate(weaponPrefab, leftWeaponSocket);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        newWeapon.Initialize(transform);
        _currentLeftWeapon = newWeapon;
    }

    public void EquipRight(Weapon weaponPrefab)
    {
        if (rightWeaponSocket == null || weaponPrefab == null)
            return;

        if (_currentRightWeapon != null)
            Destroy(_currentRightWeapon.gameObject);

        Weapon newWeapon = Instantiate(weaponPrefab, rightWeaponSocket);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        newWeapon.Initialize(transform);
        _currentRightWeapon = newWeapon;
    }

    public void AttackLeft()
    {
        _currentLeftWeapon?.Attack();
    }

    public void AttackRight()
    {
        _currentRightWeapon?.Attack();
    }

    // Legacy single-weapon attack 
    //
    // private Weapon _currentWeapon;
    //
    // public void Attack()
    // {
    //     _currentWeapon?.Attack();
    // }
}