using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private Transform weaponSocket;

    [SerializeField] private Weapon defaultWeapon;

    private Weapon _currentWeapon;

    private void Start()
    {
        if (defaultWeapon != null)
            Equip(defaultWeapon);
    }
    public void Equip(Weapon weaponPrefab)
    {
        if (_currentWeapon != null)
            Destroy(_currentWeapon.gameObject);

        Weapon newWeapon = Instantiate(weaponPrefab, weaponSocket);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        _currentWeapon = newWeapon;
    }

    public void Attack()
    {
        _currentWeapon?.Attack();
    }
}