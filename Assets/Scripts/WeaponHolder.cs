using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [Header("Socket")]
    [SerializeField] private Transform handSocket;

    [Header("Starting Weapon (Optional)")]
    [SerializeField] private GameObject startingWeaponPrefab;

    private Weapon _currentWeapon;

    public Weapon CurrentWeapon => _currentWeapon;

    private void Start()
    {
        if (startingWeaponPrefab != null)
        {
            EquipWeapon(startingWeaponPrefab);
        }
    }

    public void EquipWeapon(GameObject weaponPrefab)
    {
        if (weaponPrefab == null)
        {
            Debug.LogWarning("EquipWeapon called with null prefab.");
            return;
        }

        if (handSocket == null)
        {
            Debug.LogError("Hand socket not assigned.");
            return;
        }

        // Destroy old weapon
        if (_currentWeapon != null)
        {
            Destroy(_currentWeapon.gameObject);
            _currentWeapon = null;
        }

        // Instantiate new weapon
        GameObject weaponInstance = Instantiate(
            weaponPrefab,
            handSocket
        );

        // Reset local transform to align with socket
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.transform.localRotation = Quaternion.identity;

        // Cache weapon component
        _currentWeapon = weaponInstance.GetComponent<Weapon>();

        if (_currentWeapon == null)
        {
            Debug.LogError("Equipped prefab does not contain a Weapon component.");
            return;
        }

        // IMPORTANT: Initialize owner to prevent self-damage
        _currentWeapon.Initialize(gameObject);
    }

    public void Attack()
    {
        _currentWeapon?.Attack();
    }

    public void Unequip()
    {
        if (_currentWeapon == null) return;

        Destroy(_currentWeapon.gameObject);
        _currentWeapon = null;
    }
}